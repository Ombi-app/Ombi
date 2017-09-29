import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
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

    constructor(public notificationService: NotificationService,
                public authService: AuthService,
                private readonly router: Router,
                private readonly settingsService: SettingsService,
                private readonly jobService: JobService) { }

    public ngOnInit() {
        this.user = this.authService.claims();

        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);

        this.router.events.subscribe(() => {
            this.user = this.authService.claims();
            this.showNav = this.authService.loggedIn();
        });

        if (this.user !== null && this.user.name) {
            this.jobService.getCachedUpdate().subscribe(x => this.updateAvailable = x);
        }
    }

    public hasRole(role: string): boolean {
        return this.user.roles.some(r => r === role);
    }

    public logOut() {
        this.authService.logout();
        this.router.navigate(["login"]);
    }
}
