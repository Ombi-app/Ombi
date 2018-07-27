import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { IdentityService, PlexOAuthService, SettingsService } from "../../services";
import { AuthService } from "./../../auth/auth.service";

@Component({
    templateUrl: "./plexoauth.component.html",
})
export class PlexOAuthComponent implements OnInit {
    public pinId: number;

    constructor(private route: ActivatedRoute,
        private plexOauth: PlexOAuthService,
        private identityService: IdentityService,
        private settings: SettingsService,
        private router: Router,
        private auth: AuthService) {

        this.route.params
            .subscribe((params: any) => {
                this.pinId = params.pin;
            });
    }

    public ngOnInit(): void {
        this.plexOauth.oAuth(this.pinId).subscribe(x => {
            if (!x.accessToken) {
                return;
                // RETURN
            }

            this.identityService.createWizardUser({
                username: "",
                password: "",
                usePlexAdminAccount: true,
            }).subscribe(u => {
                if (u) {
                    this.auth.oAuth(this.pinId).subscribe(c => {
                        localStorage.setItem("id_token", c.access_token);

                        // Mark that we have done the settings now
                        this.settings.getOmbi().subscribe(ombi => {
                            ombi.wizard = true;

                            this.settings.saveOmbi(ombi).subscribe(s => {
                                this.settings.getUserManagementSettings().subscribe(usr => {

                                    usr.importPlexAdmin = true;
                                    this.settings.saveUserManagementSettings(usr).subscribe(saved => {
                                        this.router.navigate(["login"]);
                                    });
                                });
                            });
                        });
                    });
                } else {
                    //this.notificationService.error("Could not get the Plex Admin Information");
                    return;
                }
            });
        });
    }
}
