import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { IDropDownModel, ISickRageSettings } from "../../interfaces";
import { SettingsMenuComponent } from "../settingsmenu.component";
import { WikiComponent } from "../wiki.component";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule
    ],
    templateUrl: "./sickrage.component.html",
    styleUrls: ["./sickrage.component.scss"]
})
export class SickRageComponent implements OnInit {

    public qualities: IDropDownModel[];
    public form: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private fb: UntypedFormBuilder) { }

    public ngOnInit() {
        this.settingsService.getSickRageSettings()
            .subscribe(x => {
                this.form = this.fb.group({
                    enabled: [x.enabled],
                    apiKey: [x.apiKey, [Validators.required]],
                    qualityProfile: [x.qualityProfile, [Validators.required]],
                    ssl: [x.ssl],
                    subDir: [x.subDir],
                    ip: [x.ip, [Validators.required]],
                    port: [x.port, [Validators.required]],
                });
                this.qualities = x.qualities;
            });
    }

    public test(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const settings = <ISickRageSettings> form.value;
        this.testerService.sickrageTest(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully connected to SickRage!");
            } else {
                this.notificationService.error("We could not connect to SickRage!");
            }
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.settingsService.saveSickRageSettings(form.value)
            .subscribe(x => {
                if (x) {
                    this.notificationService.success("Successfully saved SickRage settings");
                } else {
                    this.notificationService.error("There was an error when saving the SickRage settings");
                }
            });
    }
}
