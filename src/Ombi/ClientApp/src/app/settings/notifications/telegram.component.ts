import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, ITelegramNotifcationSettings, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./telegram.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class TelegramComponent implements OnInit {

    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getTelegramNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                botApi: [x.botApi, [Validators.required]],
                chatId: [x.chatId, [Validators.required]],
                parseMode: [x.parseMode, [Validators.required]],

            });
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <ITelegramNotifcationSettings> form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveTelegramNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Telegram settings");
            } else {
                this.notificationService.success("There was an error when saving the Telegram settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.telegramTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Telegram message, please check the Telegram channel");
            } else {
                this.notificationService.error("There was an error when sending the Telegram message. Please check your settings");
            }
        });

    }
}
