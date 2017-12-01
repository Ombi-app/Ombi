import { Component, OnInit } from "@angular/core";
import { NavigationStart, Router } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";
import { AuthService } from "./auth/auth.service";
import { ILocalUser } from "./auth/IUserLogin";
import { NotificationService } from "./services";
import { JobService, SettingsService } from "./services";

import { ICustomizationSettings } from "./interfaces";

@Component({
    selector: "ombi",
    templateUrl: "./app.component.html",
    styleUrls: ["./app.component.scss"],
})
export class AppComponent implements OnInit {

    public customizationSettings: ICustomizationSettings;
    public user: ILocalUser;
    public showNav: boolean;
    public updateAvailable: boolean;
    public currentUrl: string;

    constructor(public notificationService: NotificationService,
                public authService: AuthService,
                private readonly router: Router,
                private readonly settingsService: SettingsService,
                private readonly jobService: JobService,
                public readonly translate: TranslateService) { 
                    this.translate.addLangs(["en", "de", "fr","da","es","it","nl","sv"]);
                    // this language will be used as a fallback when a translation isn't found in the current language
                    this.translate.setDefaultLang("en");
                    
                    // See if we can match the supported langs with the current browser lang
                    const browserLang: string = translate.getBrowserLang();
                    this.translate.use(browserLang.match(/en|fr|da|de|es|it|nl|sv/) ? browserLang : "en");
                }

    public ngOnInit() {
        this.user = this.authService.claims();

        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);

        this.router.events.subscribe((event: NavigationStart) => {
            console.log(event.url);
            this.currentUrl = event.url;
            if (event instanceof NavigationStart) {
                this.user = this.authService.claims();
                this.showNav = this.authService.loggedIn();

                if (this.user !== null && this.user.name) {
                    this.jobService.getCachedUpdate().subscribe(x => {
                        this.updateAvailable = x;
                    });
                }
            }
        });
    }

    public hasRole(role: string): boolean {
        return this.user.roles.some(r => r === role);
    }

    public logOut() {
        this.authService.logout();
        this.router.navigate(["login"]);
    }
}
