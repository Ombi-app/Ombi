import { Component, OnDestroy, OnInit } from "@angular/core";
import { Router } from "@angular/router";

import { PlatformLocation } from "@angular/common";
import { AuthService } from "../../auth/auth.service";
import { PlexOAuthService, PlexService, PlexTvService, SettingsService } from "../../services";
import { IdentityService, NotificationService } from "../../services";

@Component({
    templateUrl: "./plex.component.html",
})
export class PlexComponent implements OnInit, OnDestroy {

    public login: string;
    public password: string;
    public baseUrl: string;
    public pinTimer: any;

    private clientId: string;

    constructor(private plexService: PlexService, private router: Router,
                private notificationService: NotificationService,
                private identityService: IdentityService, private plexTv: PlexTvService,
                private settingsService: SettingsService,
                private location: PlatformLocation, private authService: AuthService,
                private plexOauth: PlexOAuthService) { }

    public ngOnInit(): void {
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
        this.settingsService.getClientId().subscribe(x => this.clientId = x);
    }

    public requestAuthToken() {
        this.plexService.logIn(this.login, this.password).subscribe(x => {
            if (x.user == null) {
                this.notificationService.error("Username or password was incorrect. Could not authenticate with Plex.");
                return;
            }

            this.identityService.createWizardUser({
                username: "",
                password: "",
                usePlexAdminAccount: true,
            }).subscribe(y => {
                if (y.result) {
                    this.router.navigate(["login"]);
                } else {
                    this.notificationService.error("Could not get the Plex Admin Information");
                    if (y.errors.length > 0) {
                        this.notificationService.error(y.errors[0]);
                    }
                    return;
                }
            });
        },
        );
    }

    public oauth() {
        const oAuthWindow = window.open(window.location.toString(), "_blank", `toolbar=0,
        location=0,
        status=0,
        menubar=0,
        scrollbars=1,
        resizable=1,
        width=500,
        height=500`);
        this.plexTv.GetPin(this.clientId, "Ombi").subscribe((pin: any) => {

            this.authService.login({ usePlexOAuth: true, password: "", rememberMe: true, username: "", plexTvPin: pin }).subscribe(x => {
                oAuthWindow!.location.replace(x.url);

                this.pinTimer = setInterval(() => {
                    // this.notify.info("Authenticating", "Loading... Please Wait");
                    this.getPinResult(x.pinId);
                }, 10000);
            });
        });
    }

    public getPinResult(pinId: number) {
        this.plexOauth.oAuth(pinId).subscribe(x => {
            if (!x.accessToken) {
                return;
                // RETURN
            }

            this.identityService.createWizardUser({
                username: "",
                password: "",
                usePlexAdminAccount: true,
            }).subscribe(u => {
                if (u.result) {
                    this.authService.oAuth(pinId).subscribe(c => {
                        localStorage.setItem("id_token", c.access_token);
                        this.router.navigate(["login"]);
                    });
                } else {

                    if (u.errors.length > 0) {
                        console.log(u.errors[0]);
                    }
                    return;
                }
            });
        });
    }

    public ngOnDestroy() {
        clearInterval(this.pinTimer);
    }
}
