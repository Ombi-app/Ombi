import { PlatformLocation } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { NavigationStart, Router } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { AuthService } from "./auth/auth.service";
import { ILocalUser } from "./auth/IUserLogin";
import { CustomPageService, IdentityService, NotificationService } from "./services";
import { JobService, SettingsService } from "./services";

import { ICustomizationSettings, ICustomPage } from "./interfaces";

@Component({
    selector: "ombi",
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

    private checkedForUpdate: boolean;

    constructor(public notificationService: NotificationService,
                public authService: AuthService,
                private readonly router: Router,
                private readonly settingsService: SettingsService,
                private readonly jobService: JobService,
                public readonly translate: TranslateService,
                private readonly identityService: IdentityService,
                private readonly platformLocation: PlatformLocation,
                private readonly customPageService: CustomPageService) {

                    const base = this.platformLocation.getBaseHrefFromDOM();
                    if (base.length > 1) {
                        __webpack_public_path__ = base + "/dist/";
                    }
                
                    this.translate.addLangs(["en", "de", "fr", "da", "es", "it", "nl", "sv", "no", "pl", "pt"]);
                    // this language will be used as a fallback when a translation isn't found in the current language
                    this.translate.setDefaultLang("en");

                    // See if we can match the supported langs with the current browser lang
                    const browserLang: string = translate.getBrowserLang();
                    this.translate.use(browserLang.match(/en|fr|da|de|es|it|nl|sv|no|pl|pt/) ? browserLang : "en");
                }

    public ngOnInit() {
        this.user = this.authService.claims();

        this.settingsService.getCustomization().subscribe(x => {
            this.customizationSettings = x;
            if(this.customizationSettings.useCustomPage) {
                this.customPageService.getCustomPage().subscribe(c => {
                    this.customPageSettings = c;
                    if(!this.customPageSettings.title) {
                        this.customPageSettings.title = "Custom Page";
                        this.customPageSettings.fontAwesomeIcon = "fa-check";
                    }
                });
            }
        });
        this.settingsService.issueEnabled().subscribe(x => this.issuesEnabled = x);
        this.settingsService.voteEnabled().subscribe(x => this.voteEnabled =x);

        this.router.events.subscribe((event: NavigationStart) => {
            this.currentUrl = event.url;
            if (event instanceof NavigationStart) {
                this.user = this.authService.claims();
                this.showNav = this.authService.loggedIn();

                // tslint:disable-next-line:no-string-literal
                if (this.user !== null && this.user.name && !this.checkedForUpdate && this.user.roles["Admin"]) {
                    this.checkedForUpdate = true;
                    this.jobService.getCachedUpdate().subscribe(x => {
                        this.updateAvailable = x;
                    },
                    err => this.checkedForUpdate = true );
                }
            }
        });
    }

    public roleClass() {
        if (this.user.roles.some(r => r === "Admin")) {
            return "adminUser";
        } else if (this.user.roles.some(r => r === "PowerUser")) {
            return "powerUser";
        }
        return "user";
    }

    public hasRole(role: string): boolean {
        return this.user.roles.some(r => r === role);
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
}
