import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IGotifyNotificationSettings, INotificationTemplates, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./gotify.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class GotifyComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getGotifyNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                baseUrl: [x.baseUrl, [Validators.required]],
                applicationToken: [x.applicationToken, [Validators.required]],
                priority: [x.priority],
            });
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IGotifyNotificationSettings> form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveGotifyNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Gotify settings");
            } else {
                this.notificationService.success("There was an error when saving the Gotify settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.gotifyTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Gotify message");
            } else {
                this.notificationService.error("There was an error when sending the Gotify message. Please check your settings");
            }
        });
    }
}
