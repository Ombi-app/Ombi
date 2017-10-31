import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./ombi.component.html",
})
export class OmbiComponent implements OnInit {

    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder) { }

    public ngOnInit() {
        this.settingsService.getOmbi().subscribe(x => {
            this.form = this.fb.group({
                collectAnalyticData: [x.collectAnalyticData],
                apiKey: [x.apiKey],
                ignoreCertificateErrors: [x.ignoreCertificateErrors],
                baseUrl: [x.baseUrl],
            });
        });
    }

    public refreshApiKey() {
        this.settingsService.resetOmbiApi().subscribe(x => {
            this.form.controls.apiKey.patchValue(x);
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.settingsService.saveOmbi(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Ombi settings");
            } else {
                this.notificationService.success("There was an error when saving the Ombi settings");
            }
        });
    }

    public successfullyCopied() {
        this.notificationService.success("Copied the Api Key to the clipboard!");
    }
}
