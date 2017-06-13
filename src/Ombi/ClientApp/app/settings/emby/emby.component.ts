import { Component, OnInit } from '@angular/core';

import { IEmbySettings } from '../../interfaces/ISettings'
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    selector: 'ombi',
    templateUrl: './emby.component.html',
})
export class EmbyComponent implements OnInit {

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    settings: IEmbySettings;

    ngOnInit(): void {
        this.settingsService.getEmby().subscribe(x => this.settings = x);
    }

    test() {
        // TODO Emby Service
    }

    save() {
        this.settingsService.saveEmby(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Emby settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Emby settings");
            }
        });
    }
}