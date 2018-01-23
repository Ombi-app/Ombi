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

    public order: string = "u.userName";
    public reverse = false;

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
        if(!user.emailAddress) {
            this.notificationService.error("The user needs an email address.");
            return;
        }
        if (!this.emailSettings.enabled) {
            this.notificationService.error("Email Notifications are not setup, cannot send welcome email");
            return;
        }
        this.identityService.sendWelcomeEmail(user).subscribe();        
        this.notificationService.success(`Sent a welcome email to ${user.emailAddress}`);
    }

    public checkAllBoxes() {
        this.checkAll = !this.checkAll;
        this.users.forEach(user => {
            user.checked = this.checkAll;
        });
    }
    
    public setOrder(value: string) {
        if (this.order === value) {
          this.reverse = !this.reverse;
        }
    
        this.order = value;
      }
}
