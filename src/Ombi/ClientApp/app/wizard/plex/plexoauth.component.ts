import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { PlatformLocation } from "@angular/common";
import { AuthService } from "../../auth/auth.service";
import { IdentityService, PlexOAuthService } from "../../services";

@Component({
    templateUrl: "./plexoauth.component.html",
})
export class PlexOAuthComponent implements OnInit {
    public pinId: number;
    public baseUrl: string;

    constructor(private route: ActivatedRoute,
                private plexOauth: PlexOAuthService,
                private identityService: IdentityService,
                private router: Router,
                private auth: AuthService,
                private location: PlatformLocation) {

        this.route.params
            .subscribe((params: any) => {
                this.pinId = params.pin;
            });
    }

    public ngOnInit(): void {
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
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
                      if (u.result) {
                          this.auth.oAuth(this.pinId).subscribe(c => {
                              localStorage.setItem("id_token", c.access_token);
                              this.router.navigate(["login"]);
                            });
                      } else {
                          
                        if(u.errors.length > 0) {
                            console.log(u.errors[0]);
                        }
                        return;
                      }
                    });
                 
        });
    }
}
