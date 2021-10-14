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

import { SignalRNotificationService } from './services/signlarnotification.service';
import { DOCUMENT } from '@angular/common';
import { CustomizationFacade } from './state/customization';


@Component({
    selector: "app-ombi",
    templateUrl: "./app.component.html",
    styleUrls: ["./app.component.scss"],
})
export class AppComponent implements OnInit {

    public customizationSettings: ICustomizationSettings;
    public customPageSettings: ICustomPage;
    public showNav: boolean;
    public updateAvailable: boolean;
    public currentUrl: string;
    public voteEnabled = false;
    public applicationName: string = "Ombi"
    public isAdmin: boolean;
    public userName: string;
    public userEmail: string;
    public accessToken: string;

    private hubConnected: boolean;

    @HostBinding('class') public componentCssClass;

    constructor(public notificationService: NotificationService,
        public authService: AuthService,
        private readonly router: Router,
        private readonly settingsService: SettingsService,
        private customizationFacade: CustomizationFacade,
        public readonly translate: TranslateService,
        private readonly customPageService: CustomPageService,
        public overlayContainer: OverlayContainer,
        private signalrNotification: SignalRNotificationService,
        private readonly snackBar: MatSnackBar,
        private readonly identity: IdentityService,
        @Inject(DOCUMENT) private document: HTMLDocument) {

        this.translate.addLangs(["da", "de", "en", "es", "fr", "it", "hu", "nl", "no", "pl", "pt", "sk", "sv", "bg", "ru"]);

        if (this.authService.loggedIn()) {
            this.identity.getAccessToken().subscribe(x => this.accessToken = x);
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
                this.userEmail = u.emailAddress;
                this.userName = u.userName;
                if (u.language) {
                    this.translate.use(u.language);
                }
            });
        }

        // this language will be used as a fallback when a translation isn't found in the current language
        this.translate.setDefaultLang("en");

        // See if we can match the supported langs with the current browser lang
        const browserLang: string = translate.getBrowserLang();
        this.translate.use(browserLang.match(/da|de|en|es|fr|it|hu|nl|no|pl|pt|sk|sv|bg|ru/) ? browserLang : "en");

    }

    public ngOnInit() {
        this.customizationFacade.settings$().subscribe(x => {
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
            }
        });
    }

    public logOut() {
        this.authService.logout();
        this.router.navigate(["login"]);
    }
}
