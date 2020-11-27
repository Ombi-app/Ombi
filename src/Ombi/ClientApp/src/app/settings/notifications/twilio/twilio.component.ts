import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, ITwilioSettings, NotificationType } from "../../../interfaces";
import { TesterService } from "../../../services";
import { NotificationService } from "../../../services";
import { SettingsService } from "../../../services";

@Component({
    templateUrl: "./twilio.component.html",
})
export class TwilioComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
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

    public onSubmit(form: FormGroup) {
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
