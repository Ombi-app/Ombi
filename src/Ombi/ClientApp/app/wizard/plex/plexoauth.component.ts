import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { ConfirmationService } from "primeng/primeng";

import { PlexOAuthService, IdentityService, SettingsService } from "../../services";
import { AuthService } from "./../../auth/auth.service";

@Component({
    templateUrl: "./plexoauth.component.html",
})
export class PlexOAuthComponent implements OnInit {
    public pinId: number;

    constructor(private route: ActivatedRoute,
                private plexOauth: PlexOAuthService,
                private confirmationService: ConfirmationService,
                private identityService: IdentityService,
                private settings: SettingsService,
                private router: Router,
                private auth: AuthService) {

        this.route.params
            .subscribe((params: any) => {
                this.pinId = params.pin;
            });
    }
            
    ngOnInit(): void {
        this.plexOauth.oAuth(this.pinId).subscribe(x => {
            x.accessToken;  
            
            this.confirmationService.confirm({
                message: "Do you want your Plex user to be the main admin account on Ombi?",
                header: "Use Plex Account",
                icon: "fa fa-check",
                accept: () => {
                    this.identityService.createWizardUser({
                      username: "",
                      password: "",
                      usePlexAdminAccount: true,
                    }).subscribe(x => {
                      if (x) {
                          this.auth.oAuth(this.pinId).subscribe(c => {
                              localStorage.setItem("id_token", c.access_token);
                    
                              // Mark that we have done the settings now
                              this.settings.getOmbi().subscribe(ombi => {
                                  ombi.wizard = true;
  
                                  this.settings.saveOmbi(ombi).subscribe(x => {
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
                },
                reject: () => {
                  this.router.navigate(["Wizard/CreateAdmin"]);
                },
              });  
        });
    }
    
}
