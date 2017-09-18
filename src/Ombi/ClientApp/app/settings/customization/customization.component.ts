import { Component, OnInit } from "@angular/core";

import { ICustomizationSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./customization.component.html",
})
export class CustomizationComponent implements OnInit {

    public settings: ICustomizationSettings;

    constructor(private settingsService: SettingsService, private notificationService: NotificationService) {  }

    public ngOnInit() {

        this.settingsService.getCustomization().subscribe(x => this.settings = x);
    }

    public save() {
        this.settingsService.saveCustomization(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Ombi settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Ombi settings");
            }
        });
    }
}
