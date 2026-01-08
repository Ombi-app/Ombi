import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { INewsletterNotificationSettings, NotificationType } from "../../interfaces";
import { JobService, NotificationService, SettingsService } from "../../services";
import { TesterService } from "../../services/applications/tester.service";
import { SettingsMenuComponent } from "../settingsmenu.component";
import { WikiComponent } from "../wiki.component";

@Component({
    standalone: true,
    templateUrl: "./newsletter.component.html",
    styleUrls: ["./notificationtemplate.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        SettingsMenuComponent,
        WikiComponent
    ]
})
export class NewsletterComponent implements OnInit {

    public NotificationType = NotificationType;
    public settings: INewsletterNotificationSettings;
    public emailToAdd: string;

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
    public addEmail() {

        if (this.emailToAdd) {
            const emailRegex = "[a-zA-Z0-9.-_]{1,}@[a-zA-Z.-]{2,}[.]{1}[a-zA-Z]{2,}";
            const match = this.emailToAdd.match(emailRegex)!;
            if (match && match.length > 0) {
                this.settings.externalEmails.push(this.emailToAdd);
                this.emailToAdd = "";
            } else {
                this.notificationService.error("Please enter a valid email address");
            }
        }
    }

    public deleteEmail(email: string) {
        const index = this.settings.externalEmails.indexOf(email);    // <-- Not supported in <IE9
        if (index !== -1) {
                this.settings.externalEmails.splice(index, 1);
            }
    }
}
