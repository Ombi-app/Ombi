import { Component, OnInit } from '@angular/core';

import { ILandingPageSettings } from '../../interfaces/ISettings'
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";

@Component({
  
    templateUrl: './landingpage.component.html',
})
export class LandingPageComponent implements OnInit {

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    settings: ILandingPageSettings;

    ngOnInit(): void {
        this.settingsService.getLandingPage().subscribe(x => {
            this.settings = x
        });
    }

    save() {
        this.settingsService.saveLandingPage(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Landing Page settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Landing Page settings");
            }
        });
    }
}