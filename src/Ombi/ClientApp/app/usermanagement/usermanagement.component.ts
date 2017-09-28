import { Component, OnInit } from "@angular/core";

import { ICustomizationSettings, IEmailNotificationSettings, IUser } from "../interfaces";
import { IdentityService, NotificationService, SettingsService } from "../services";

@Component({
    templateUrl: "./usermanagement.component.html",
})
export class UserManagementComponent implements OnInit {

    public users: IUser[];
    public checkAll = false;
    public emailSettings: IEmailNotificationSettings; 
    public customizationSettings: ICustomizationSettings;

    constructor(private readonly identityService: IdentityService,
                private readonly settingsService: SettingsService,
                private readonly notificationService: NotificationService) { }

    public ngOnInit() {
        this.users = [];
        this.identityService.getUsers().subscribe(x => {
            this.users = x;
        });

        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getEmailNotificationSettings().subscribe(x => this.emailSettings = x);
    }

    public welcomeEmail(user: IUser) {
        if (!this.emailSettings.enabled) {
            this.notificationService.error("Email", "Email Notifications are not setup, cannot send welcome email");
            return;
        }
        this.identityService.sendWelcomeEmail(user).subscribe();
    }

    public checkAllBoxes() {
        this.checkAll = !this.checkAll;
        this.users.forEach(user => {
            user.checked = this.checkAll;
        });
    }
}
