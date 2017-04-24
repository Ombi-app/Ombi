import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { IdentityService } from '../../services/identity.service';
import { SettingsService } from '../../services/settings.service';
import { AuthService } from '../../auth/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './createadmin.component.html',
})
export class CreateAdminComponent {

    constructor(private identityService: IdentityService, private notificationService: NotificationService,
        private router: Router, private auth: AuthService, private settings: SettingsService) { }


    username: string;
    password: string;

    createUser() {
        this.identityService.createUser(this.username, this.password).subscribe(x => {
            if (x) {
                // Log me in.
                this.auth.login({ username: this.username, password: this.password }).subscribe(c => {

                    localStorage.setItem("id_token", c.access_token);

                    // Mark that we have done the settings now
                    this.settings.getOmbi().subscribe(ombi => {
                        ombi.wizard = true;

                        this.settings.saveOmbi(ombi).subscribe(x => {
                            
                            this.router.navigate(['search']);
                        });

                    });

                });
            } else {
                this.notificationService.error("Error in creating user",
                    "There was an error... You might want to put this on Github...");
            }
        });
    }
}