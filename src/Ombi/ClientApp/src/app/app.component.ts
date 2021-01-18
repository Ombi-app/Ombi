import { OverlayContainer } from '@angular/cdk/overlay';

import { Component, OnInit, HostBinding, Inject } from "@angular/core";
import { NavigationStart, Router } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { AuthService } from "./auth/auth.service";
import { ILocalUser } from "./auth/IUserLogin";
import { NotificationService, CustomPageService, IdentityService } from "./services";
import { SettingsService } from "./services";
import { MatSnackBar } from '@angular/material/snack-bar';

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
    public user: ILocalUser;
    public showNav: boolean;
    public updateAvailable: boolean;
    public currentUrl: string;
    public userAccessToken: string;
    public voteEnabled = false;
    public applicationName: string = "Ombi"
    public isAdmin: boolean;
    public username: string;

    private hubConnected: boolean;

    @HostBinding('class') public componentCssClass;

    constructor(public notificationService: NotificationService,
        public authService: AuthService,
        private readonly router: Router,
        private readonly settingsService: SettingsService,
        public readonly translate: TranslateService,
        private readonly customPageService: CustomPageService,
        public overlayContainer: OverlayContainer,
        private storage: StorageService,
        private signalrNotification: SignalRNotificationService,
        private readonly snackBar: MatSnackBar,
        private readonly identity: IdentityService,
        @Inject(DOCUMENT) private document: HTMLDocument) {

        this.translate.addLangs(["en", "de", "fr", "da", "es", "it", "nl", "sk", "sv", "no", "pl", "pt"]);

        if (this.authService.loggedIn()) {
            this.user = this.authService.claims();
            this.username = this.user.name;
            if (!this.hubConnected) {
                this.signalrNotification.initialize();
                this.hubConnected = true;

                this.signalrNotification.Notification.subscribe(data => {
                    this.snackBar.open(data, "OK", {
                        duration: 3000
                    });
                });
            }
            this.identity.getUser().subscribe(u => {
                if (u.language) {
                    this.translate.use(u.language);
                }
            });
        }

        // this language will be used as a fallback when a translation isn't found in the current language
        this.translate.setDefaultLang("en");

        // See if we can match the supported langs with the current browser lang
        const browserLang: string = translate.getBrowserLang();
        this.translate.use(browserLang.match(/en|fr|da|de|es|it|nl|sk|sv|no|pl|pt/) ? browserLang : "en");

    }

    public ngOnInit() {
        window["loading_screen"].finish();

        this.settingsService.getCustomization().subscribe(x => {
            this.customizationSettings = x;

            if (this.customizationSettings && this.customizationSettings.applicationName) {
                this.applicationName = this.customizationSettings.applicationName;
                this.document.getElementsByTagName('title')[0].innerText = this.applicationName;
            }
            if (this.customizationSettings && this.customizationSettings.customCss) {
                var dom = this.document.getElementsByTagName('head')[0];
                var css = document.createElement("style");
                css.innerHTML = this.customizationSettings.customCss;
                dom.appendChild(css);
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
        this.settingsService.voteEnabled().subscribe(x => this.voteEnabled = x);

        this.router.events.subscribe((event: NavigationStart) => {
            this.currentUrl = event.url;
            if (event instanceof NavigationStart) {
                this.isAdmin = this.authService.hasRole("admin");
                this.showNav = this.authService.loggedIn();
                if (this.showNav) {
                    this.user = this.authService.claims();
                    this.username = this.user.name;
                }
            }
        });
    }

    public logOut() {
        this.authService.logout();
        this.router.navigate(["login"]);
    }
}
