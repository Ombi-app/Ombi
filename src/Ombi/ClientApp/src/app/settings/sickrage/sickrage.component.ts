import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IDropDownModel, ISickRageSettings } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./sickrage.component.html",
    styleUrls: ["./sickrage.component.scss"]
})
export class SickRageComponent implements OnInit {

    public qualities: IDropDownModel[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private fb: FormBuilder) { }

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

    public test(form: FormGroup) {
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

    public onSubmit(form: FormGroup) {
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
