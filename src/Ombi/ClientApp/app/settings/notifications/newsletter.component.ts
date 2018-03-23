import {  } from './../../services/job.service';

import { Component, OnInit } from "@angular/core";

import { INewsletterNotificationSettings, NotificationType } from "../../interfaces";
import { JobService, NotificationService, SettingsService } from "../../services";
import { TesterService } from "./../../services/applications/tester.service";

@Component({
    templateUrl: "./newsletter.component.html",
})
export class NewsletterComponent implements OnInit {

    public NotificationType = NotificationType;
    public settings: INewsletterNotificationSettings;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private testService: TesterService,
                private jobService: JobService) { }

    public ngOnInit() {
        this.settingsService.getNewsletterSettings().subscribe(x => {
            this.settings = x;
        });
    }

    public updateDatabase() {
        this.settingsService.updateNewsletterDatabase().subscribe();
        this.notificationService.success("Database updated");
    }

    public test() {
        this.testService.newsletterTest(this.settings).subscribe();
        this.notificationService.success("Test message queued");
    }

    public trigger() {
        this.jobService.runNewsletter().subscribe();
        this.notificationService.success("Triggered newsletter job");
    }

    public onSubmit() {
              this.settingsService.saveNewsletterSettings(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Newsletter settings");
            } else {
                this.notificationService.error("There was an error when saving the Newsletter settings");
            }
        });

    }
}
