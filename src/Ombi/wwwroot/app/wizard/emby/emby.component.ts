import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { EmbyService } from '../../services/applications/emby.service';
import { NotificationService } from '../../services/notification.service';

import { IEmbySettings } from '../../interfaces/ISettings';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './emby.component.html',
})
export class EmbyComponent {

    constructor(private embyService: EmbyService,
        private router: Router,
        private notificationService: NotificationService) {

        this.embySettings = {
            administratorId: "",
            apiKey: "",
            enable: true,
            enableEpisodeSearching: true,
            id: 0,
            ip: "",
            port: 8096,
            ssl: false,
            subDir: "",
        }
    }

    private embySettings: IEmbySettings;


    save() {
        this.embyService.logIn(this.embySettings).subscribe(x => {
            if (x == null || !x.apiKey) {
                this.notificationService.error("Could Not Authenticate", "Username or password was incorrect. Could not authenticate with Emby.");
                return;
            }
            this.router.navigate(['Wizard/CreateAdmin']);
        });
    }
}