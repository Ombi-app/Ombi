import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, ReactiveFormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { ClipboardModule } from "@angular/cdk/clipboard";
import { MatIconModule } from "@angular/material/icon";

import { Branch, ILanguageRefine, IOmbiSettings } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

import languageData from "./../../../other/iso-lang.json";

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
        TranslateModule,
        ClipboardModule,
        MatIconModule,
    ],
    templateUrl: "./ombi.component.html",
    styleUrls: ["./ombi.component.scss"],
})
export class OmbiComponent implements OnInit {

    public form: UntypedFormGroup;
    public langauges: ILanguageRefine[];
    public Branch = Branch;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: UntypedFormBuilder) { }

    public ngOnInit() {
        this.settingsService.getOmbi().subscribe(x => {
            this.form = this.fb.group({
                collectAnalyticData: [x.collectAnalyticData],
                apiKey: [x.apiKey],
                baseUrl: [x.baseUrl],
                doNotSendNotificationsForAutoApprove: [x.doNotSendNotificationsForAutoApprove],
                hideRequestsUsers: [x.hideRequestsUsers],
                defaultLanguageCode: [x.defaultLanguageCode],
                disableHealthChecks: [x.disableHealthChecks],
                autoDeleteAvailableRequests: [x.autoDeleteAvailableRequests],
                autoDeleteAfterDays: [x.autoDeleteAfterDays],
                branch: [x.branch]
            });
        });
        this.langauges = <ILanguageRefine[]>languageData
    }

    public refreshApiKey() {
        this.settingsService.resetOmbiApi().subscribe(x => {
            this.form.controls.apiKey.patchValue(x);
        });
    }

    public onSubmit(form: UntypedFormGroup) {
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
