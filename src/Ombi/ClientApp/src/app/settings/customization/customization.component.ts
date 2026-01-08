import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { CustomizationFacade } from "../../state/customization";
import { ICustomizationSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { WikiComponent } from "../wiki.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        WikiComponent
    ],
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
