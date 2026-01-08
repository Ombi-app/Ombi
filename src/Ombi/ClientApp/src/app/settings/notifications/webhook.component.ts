import { Component, OnInit } from "@angular/core";
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, IWebhookNotificationSettings, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { NotificationTemplate } from "./notificationtemplate.component";

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
        NotificationTemplate
    ],
    templateUrl: "./webhook.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class WebhookComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: UntypedFormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getWebhookNotificationSettings().subscribe(x => {
            this.form = this.fb.group({
                enabled: [x.enabled],
                webhookUrl: [x.webhookUrl, [Validators.required]],
                applicationToken: [x.applicationToken],
            });
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IWebhookNotificationSettings> form.value;

        this.settingsService.saveWebhookNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Webhook settings");
            } else {
                this.notificationService.success("There was an error when saving the Webhook settings");
            }
        });

    }

    public test(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.webhookTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Webhook message");
            } else {
                this.notificationService.error("There was an error when sending the Webhook message. Please check your settings");
            }
        });
    }
}
