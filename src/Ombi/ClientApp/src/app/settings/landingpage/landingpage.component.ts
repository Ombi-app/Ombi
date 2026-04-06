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

import { ILandingPageSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
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
    ],
    templateUrl: "./landingpage.component.html",
    styleUrls: ["./landingpage.component.scss"],
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
                this.notificationService.success("Successfully saved the Landing Page settings");
            } else {
                this.notificationService.success("There was an error when saving the Landing Page settings");
            }
        });
    }
}
