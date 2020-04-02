import { OverlayContainer } from '@angular/cdk/overlay';

import { Component, OnInit, HostBinding, Inject } from "@angular/core";
import { NavigationStart, Router } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { AuthService } from "./auth/auth.service";
import { ILocalUser } from "./auth/IUserLogin";
import { IdentityService, NotificationService, CustomPageService } from "./services";
import { JobService, SettingsService } from "./services";
import { MatSnackBar } from '@angular/material';

import { ICustomizationSettings, ICustomPage } from "./interfaces";
import { StorageService } from './shared/storage/storage-service';

import { SignalRNotificationService } from './services/signlarnotification.service';
import { DOCUMENT } from '@angular/common';


@Component({
    selector: "app-ombi",
    templateUrl: "./app.component.html",
    styleUrls: ["./app.component.scss"],
})
export class AppComponent implements OnInit {

    public customizationSettings: ICustomizationSettings;
    public customPageSettings: ICustomPage;
    public issuesEnabled = false;
    public user: ILocalUser;
    public showNav: boolean;
    public updateAvailable: boolean;
    public currentUrl: string;
    public userAccessToken: string;
    public voteEnabled = false;
    public applicationName: string = "Ombi"
    public isAdmin: boolean;
    public username: string;

    private checkedForUpdate: boolean;
    private hubConnected: boolean;

    

    @HostBinding('class') public componentCssClass;

    constructor(public notificationService: NotificationService,
        public authService: AuthService,
        private readonly router: Router,
        private readonly settingsService: SettingsService,
        private readonly jobService: JobService,
        public readonly translate: TranslateService,
        private readonly identityService: IdentityService,
        private readonly customPageService: CustomPageService,
        public overlayContainer: OverlayContainer,
        private storage: StorageService,
        private signalrNotification: SignalRNotificationService,
        private readonly snackBar: MatSnackBar,
        @Inject(DOCUMENT) private document: HTMLDocument) {      

        this.translate.addLangs(["en", "de", "fr", "da", "es", "it", "nl", "sk", "sv", "no", "pl", "pt"]);

        const selectedLang = this.storage.get("Language");

        // this language will be used as a fallback when a translation isn't found in the current language
        this.translate.setDefaultLang("en");
        if (selectedLang) {
            this.translate.use(selectedLang);
        } else {
            // See if we can match the supported langs with the current browser lang
            const browserLang: string = translate.getBrowserLang();
            this.translate.use(browserLang.match(/en|fr|da|de|es|it|nl|sk|sv|no|pl|pt/) ? browserLang : "en");
        }
    }

    public ngOnInit() {
        window["loading_screen"].finish();
        const theme = this.storage.get("theme");
        this.onSetTheme(theme);

        this.settingsService.getCustomization().subscribe(x => {
            this.customizationSettings = x;

            if (this.customizationSettings && this.customizationSettings.applicationName) {
                this.applicationName = this.customizationSettings.applicationName;
                debugger;
                this.document.getElementsByTagName('title')[0].innerText = this.applicationName;
            }

            if (this.customizationSettings.useCustomPage) {
                this.customPageService.getCustomPage().subscribe(c => {
                    this.customPageSettings = c;
                    if (!this.customPageSettings.title) {
                        this.customPageSettings.title = "Custom Page";
                        this.customPageSettings.fontAwesomeIcon = "fa-check";
                    }
                });
            }
        });
        this.settingsService.issueEnabled().subscribe(x => this.issuesEnabled = x);
        this.settingsService.voteEnabled().subscribe(x => this.voteEnabled = x);

        this.router.events.subscribe((event: NavigationStart) => {
            this.currentUrl = event.url;
            if (event instanceof NavigationStart) {
                this.user = this.authService.claims();
                if (this.user && this.user.username) {
                    this.username = this.user.username;
                }
                this.isAdmin = this.authService.hasRole("admin");
                this.showNav = this.authService.loggedIn();

                // tslint:disable-next-line:no-string-literal
                if (this.user !== null && this.user.name && !this.checkedForUpdate && this.isAdmin) {
                    this.checkedForUpdate = true;
                    this.jobService.getCachedUpdate().subscribe(x => {
                        this.updateAvailable = x;
                    },
                        err => this.checkedForUpdate = true);
                }

                if (this.authService.loggedIn() && !this.hubConnected) {
                    this.signalrNotification.initialize();
                    this.hubConnected = true;

                    this.signalrNotification.Notification.subscribe(data => {
                        this.snackBar.open(data, "OK", {
                            duration: 3000
                        });
                    });
                }
            }
        });
    }

    public openMobileApp(event: any) {
        event.preventDefault();
        if (!this.customizationSettings.applicationUrl) {
            this.notificationService.warning("Mobile", "Please ask your admin to setup the Application URL!");
            return;
        }

        this.identityService.getAccessToken().subscribe(x => {
            const url = `ombi://${this.customizationSettings.applicationUrl}_${x}`;
            window.location.assign(url);
        });
    }

    public logOut() {
        this.authService.logout();
        this.router.navigate(["login"]);
    }

    public onSetTheme(theme: string) {
        if (theme) {
            this.overlayContainer.getContainerElement().classList.add(theme);
            this.componentCssClass = theme;
        }
    }
}
