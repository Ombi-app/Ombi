import { Component, OnInit } from '@angular/core';

import { IEmailNotificationSettings, NotificationType } from '../../interfaces/INotifcationSettings';
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    templateUrl: './emailnotification.component.html',
})
export class EmailNotificationComponent implements OnInit {

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    settings: IEmailNotificationSettings;
    NotificationType = NotificationType;

    ngOnInit(): void {
        this.settingsService.getEmailNotificationSettings().subscribe(x => this.settings = x);
    }

    test() {
        // TODO Emby Service
    }

    save() {
        this.settingsService.saveEmailNotificationSettings(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Email settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Email settings");
            }
        });
    }
}