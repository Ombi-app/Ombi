import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatChipsModule } from "@angular/material/chips";
import { MatDividerModule } from "@angular/material/divider";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { INewsletterNotificationSettings, INotificationTemplates } from "../../interfaces";
import { JobService, NotificationService, SettingsService, TesterService } from "../../services";
import { NotificationBaseComponent } from "./shared/notification-base.component";
import { NotificationShellComponent } from "./shared/notification-shell.component";

const EMAIL_REGEX = /^[a-zA-Z0-9._-]+@[a-zA-Z.-]{2,}\.[a-zA-Z]{2,}$/;

@Component({
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCardModule,
        MatChipsModule,
        MatDividerModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationShellComponent,
    ],
    templateUrl: "./newsletter.component.html",
    styleUrls: ["./newsletter.component.scss"],
})
export class NewsletterComponent extends NotificationBaseComponent<INewsletterNotificationSettings> {

    protected readonly providerName = "Newsletter";
    protected override readonly attachTemplates = false;

    public emailToAdd = "";
    public externalEmails: string[] = [];
    private originalTemplate: INotificationTemplates | undefined;

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService,
                private readonly jobService: JobService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected override get savedMessage(): string {
        return "Successfully saved the Newsletter settings";
    }
    protected override get saveErrorMessage(): string {
        return "There was an error when saving the Newsletter settings";
    }
    protected override get testSuccessMessage(): string {
        return "Test newsletter queued. It will be sent to admin users with valid email addresses.";
    }

    protected loadSettings(): Observable<INewsletterNotificationSettings> {
        return this.settingsService.getNewsletterSettings();
    }

    protected saveSettings(settings: INewsletterNotificationSettings): Observable<boolean> {
        const merged: INewsletterNotificationSettings = {
            ...settings,
            externalEmails: [...this.externalEmails],
            notificationTemplate: {
                ...(this.originalTemplate ?? {} as INotificationTemplates),
                subject: settings.notificationTemplate?.subject ?? "",
                message: settings.notificationTemplate?.message ?? "",
            },
        };
        return this.settingsService.saveNewsletterSettings(merged);
    }

    protected testSettings(_settings: INewsletterNotificationSettings): Observable<boolean> {
        return this.testerService.newsletterTest(_settings);
    }

    protected buildForm(x: INewsletterNotificationSettings) {
        return {
            enabled: [x.enabled],
            disableTv: [x.disableTv],
            disableMovies: [x.disableMovies],
            disableMusic: [x.disableMusic],
            notificationTemplate: this.fb.group({
                subject: [x.notificationTemplate?.subject, [Validators.required]],
                message: [x.notificationTemplate?.message, [Validators.required]],
            }),
        };
    }

    protected override onFormReady(_form: UntypedFormGroup, settings: INewsletterNotificationSettings): void {
        this.originalTemplate = settings.notificationTemplate;
        this.externalEmails = [...(settings.externalEmails ?? [])];
    }

    public updateDatabase(): void {
        this.settingsService.updateNewsletterDatabase().subscribe(() => {
            this.notificationService.success("Newsletter database updated");
        });
    }

    public triggerNow(): void {
        this.jobService.runNewsletter().subscribe(() => {
            this.notificationService.success("Newsletter job triggered");
        });
    }

    public addEmail(): void {
        const candidate = this.emailToAdd?.trim();
        if (!candidate) {
            return;
        }
        if (!EMAIL_REGEX.test(candidate)) {
            this.notificationService.error("Please enter a valid email address");
            return;
        }
        if (this.externalEmails.includes(candidate)) {
            this.notificationService.warning("Already added", "That email address is already in the list");
            return;
        }
        this.externalEmails = [...this.externalEmails, candidate];
        this.emailToAdd = "";
        this.form?.markAsDirty();
    }

    public removeEmail(email: string): void {
        this.externalEmails = this.externalEmails.filter(e => e !== email);
        this.form?.markAsDirty();
    }
}
