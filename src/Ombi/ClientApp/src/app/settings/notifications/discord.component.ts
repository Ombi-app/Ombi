import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IDiscordNotifcationSettings, INotificationTemplates, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./discord.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class DiscordComponent implements OnInit {

    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getDiscordNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                username: [x.username],
                webhookUrl: [x.webhookUrl, [Validators.required]],
                icon: [x.icon]

            });
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IDiscordNotifcationSettings> form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveDiscordNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Discord settings");
            } else {
                this.notificationService.success("There was an error when saving the Discord settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.discordTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Discord message, please check the discord channel");
            } else {
                this.notificationService.error("There was an error when sending the Discord message. Please check your settings");
            }
        });

    }
}
