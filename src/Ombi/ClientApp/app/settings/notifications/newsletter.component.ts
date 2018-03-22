import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { INewsletterNotificationSettings, INotificationTemplates, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./newsletter.component.html",
})
export class NewsletterComponent implements OnInit {

    public NotificationType = NotificationType;
    public template: INotificationTemplates;
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getNewsletterSettings().subscribe(x => {
            this.template = x.notificationTemplate;

            this.form = this.fb.group({
                enabled: [x.enabled],
            });
        });
    }

    public updateDatabase() {
        this.settingsService.updateNewsletterDatabase().subscribe();
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <INewsletterNotificationSettings>form.value;
        settings.notificationTemplate = this.template;

        this.settingsService.saveNewsletterSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Newsletter settings");
            } else {
                this.notificationService.error("There was an error when saving the Newsletter settings");
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
