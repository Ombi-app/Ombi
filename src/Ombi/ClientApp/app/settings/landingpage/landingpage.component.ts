import { Component, OnInit } from "@angular/core";

import { ILandingPageSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./landingpage.component.html",
})
export class LandingPageComponent implements OnInit {

    public settings: ILandingPageSettings;

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    public ngOnInit() {
        this.settingsService.getLandingPage().subscribe(x => {
            this.settings = x;
        });
    }

    public save() {
        this.settingsService.saveLandingPage(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved the Landing Page settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Landing Page settings");
            }
        });
    }
}
