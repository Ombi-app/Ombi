import { Component, OnInit } from "@angular/core";

import { CustomizationFacade } from "../../state/customization";
import { ICustomizationSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./customization.component.html",
    styleUrls: ["./customization.component.scss"],
})
export class CustomizationComponent implements OnInit {

    public settings: ICustomizationSettings;
    public advanced: boolean;

    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private customizationFacade: CustomizationFacade) {  }

    public ngOnInit() {
        this.customizationFacade.settings$().subscribe(x => {
            this.settings = { ...x };
        });
    }

    public save() {

        this.settingsService.verifyUrl(this.settings.applicationUrl).subscribe(x => {
            if (this.settings.applicationUrl) {
                if (!x) {
                    this.notificationService.error(`The URL "${this.settings.applicationUrl}" is not valid. Please format it correctly e.g. http://www.google.com/`);
                    return;
                }
            }

            this.customizationFacade.saveSettings(this.settings).subscribe(x => {
                if (x) {
                    this.notificationService.success("Successfully saved Ombi settings");
                } else {
                    this.notificationService.success("There was an error when saving the Ombi settings");
                }
            });
        });

    }
}
