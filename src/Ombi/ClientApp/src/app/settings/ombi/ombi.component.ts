import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { ILanguageRefine, IOmbiSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

import languageData from "./../../../other/iso-lang.json";

@Component({
    templateUrl: "./ombi.component.html",
    styleUrls: ["./ombi.component.scss"],
})
export class OmbiComponent implements OnInit {

    public form: FormGroup;
    public langauges: ILanguageRefine[];

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
                doNotSendNotificationsForAutoApprove: [x.doNotSendNotificationsForAutoApprove],
                hideRequestsUsers: [x.hideRequestsUsers],
                defaultLanguageCode: [x.defaultLanguageCode],
                disableHealthChecks: [x.disableHealthChecks],
                autoDeleteAvailableRequests: [x.autoDeleteAvailableRequests],
                autoDeleteAfterDays: [x.autoDeleteAfterDays]
            });
        });
        this.langauges = <ILanguageRefine[]>languageData
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

        const result = <IOmbiSettings> form.value;
        if (result.baseUrl  && result.baseUrl.length > 0) {
            if (!result.baseUrl.startsWith("/")) {
                this.notificationService.error("Please ensure your base url starts with a '/'");
                return;
            }
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
