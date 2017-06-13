import { Component, OnInit } from '@angular/core';

import { ICustomizationSettings } from '../../interfaces/ISettings'
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    selector: 'ombi',
    templateUrl: './customization.component.html',
})
export class CustomizationComponent implements OnInit {

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    settings: ICustomizationSettings;

    ngOnInit(): void {

        this.settingsService.getCustomization().subscribe(x => this.settings = x);
    }

    save() {
        this.settingsService.saveCustomization(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Ombi settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Ombi settings");
            }
        });
    }
}