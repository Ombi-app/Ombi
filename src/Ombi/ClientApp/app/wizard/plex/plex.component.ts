import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { PlexService } from '../../services/applications/plex.service';
import { NotificationService } from '../../services/notification.service';

import { IPlexAuthentication } from '../../interfaces/IPlex';

@Component({
    selector: 'ombi',
    templateUrl: './plex.component.html',
})
export class PlexComponent {

    constructor(private plexService: PlexService, private router: Router, private notificationService: NotificationService) { }

    private authenticationResult: IPlexAuthentication;

    login: string;
    password: string;

    requestAuthToken() {
        this.plexService.logIn(this.login, this.password).subscribe(x => {
            if (x.user == null) {
                this.notificationService.error("Could Not Authenticate", "Username or password was incorrect. Could not authenticate with Plex.");
                return;
            }
            this.authenticationResult = x;

            this.router.navigate(['Wizard/CreateAdmin']);
        });
    }
}