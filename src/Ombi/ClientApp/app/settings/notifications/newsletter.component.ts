
import { Component, OnInit } from "@angular/core";

import { INewsletterNotificationSettings, NotificationType } from "../../interfaces";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { TesterService } from "./../../services/applications/tester.service";

@Component({
    templateUrl: "./newsletter.component.html",
})
export class NewsletterComponent implements OnInit {

    public NotificationType = NotificationType;
    public settings: INewsletterNotificationSettings;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private testService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getNewsletterSettings().subscribe(x => {
            this.settings = x;
        });
    }

    public updateDatabase() {
        this.settingsService.updateNewsletterDatabase().subscribe();
    }

    public test() {
        this.testService.newsletterTest(this.settings).subscribe();
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
