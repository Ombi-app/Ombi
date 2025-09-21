import { Component, OnInit } from "@angular/core";
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, ITwilioSettings, NotificationType } from "../../../interfaces";
import { TesterService } from "../../../services";
import { NotificationService } from "../../../services";
import { SettingsService } from "../../../services";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { NotificationTemplate } from "../notificationtemplate.component";
import { WhatsAppComponent } from "./whatsapp.component";
import { MatTabsModule } from "@angular/material/tabs";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatTabsModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        WhatsAppComponent
    ],
    templateUrl: "./twilio.component.html",
})
export class TwilioComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: UntypedFormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getTwilioSettings().subscribe(x => {
            this.templates = x.whatsAppSettings.notificationTemplates;

            this.form = this.fb.group({
                whatsAppSettings: this.fb.group({
                    enabled: [x.whatsAppSettings.enabled],
                    accountSid: [x.whatsAppSettings.accountSid],
                    authToken: [x.whatsAppSettings.authToken],
                    from: [x.whatsAppSettings.from],
                }),
            });
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <ITwilioSettings> form.value;
        settings.whatsAppSettings.notificationTemplates = this.templates;

        this.settingsService.saveTwilioSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Twilio settings");
            } else {
                this.notificationService.success("There was an error when saving the Twilio settings");
            }
        });

    }
}
