import { Component, OnInit } from '@angular/core';

import { IOmbiSettings } from '../../interfaces/ISettings'
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    templateUrl: './ombi.component.html',
})
export class OmbiComponent implements OnInit {

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    settings: IOmbiSettings;

    ngOnInit(): void {
        this.settings = {
            apiKey: "",
            port: 3579,
            wizard: true,
            collectAnalyticData: true,
            id:0
        }
        this.settingsService.getOmbi().subscribe(x => this.settings = x);
    }


    refreshApiKey() {
        
    }

    save() {
        this.settingsService.saveOmbi(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Ombi settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Ombi settings");
            }
        });
    }
}