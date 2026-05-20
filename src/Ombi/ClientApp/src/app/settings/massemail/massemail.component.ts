import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatDividerModule } from "@angular/material/divider";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { IMassEmailModel, IMassEmailUserModel } from "../../interfaces";
import { IdentityService, NotificationMessageService, NotificationService, SettingsService } from "../../services";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCardModule,
        MatCheckboxModule,
        MatDividerModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatProgressBarModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
    ],
    providers: [NotificationMessageService],
    templateUrl: "./massemail.component.html",
    styleUrls: ["./massemail.component.scss"],
})
export class MassEmailComponent implements OnInit {

    public users: IMassEmailUserModel[] = [];
    public message = "";
    public subject = "";
    public bcc = false;
    public sending = false;

    public missingSubject = false;

    public emailEnabled = false;
    public loadingUsers = true;

    constructor(private readonly notification: NotificationService,
                private readonly identityService: IdentityService,
                private readonly notificationMessageService: NotificationMessageService,
                private readonly settingsService: SettingsService) {
    }

    public ngOnInit(): void {
        this.identityService.getUsers().subscribe(x => {
            this.users = x
                .filter(u => !!u.emailAddress)
                .map(u => ({ user: u, selected: false }));
            this.loadingUsers = false;
        });
        this.settingsService.getEmailSettingsEnabled().subscribe(x => this.emailEnabled = x);
    }

    public selectAllUsers(event: { checked: boolean }): void {
        this.users.forEach(u => u.selected = event.checked);
    }

    public get selectedCount(): number {
        return this.users.filter(u => u.selected).length;
    }

    public get allSelected(): boolean {
        return this.users.length > 0 && this.selectedCount === this.users.length;
    }

    public send(): void {
        if (!this.subject) {
            this.missingSubject = true;
            return;
        }
        this.missingSubject = false;

        const selectedUsers = this.users.filter(u => u.selected).map(u => u.user);
        if (selectedUsers.length <= 0) {
            this.notification.error("You need to select at least one user to send the email");
            return;
        }

        const model = <IMassEmailModel>{
            users: selectedUsers,
            subject: this.subject,
            body: this.message,
            bcc: this.bcc,
        };

        this.sending = true;
        this.notification.info("Sending", "Sending mass email... Please wait");
        this.notificationMessageService.sendMassEmail(model).subscribe({
            next: () => this.notification.success("We have sent the mass email to the users selected!"),
            complete: () => this.sending = false,
            error: () => this.sending = false,
        });
    }
}
