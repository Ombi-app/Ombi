﻿import { Component, OnInit } from "@angular/core";

import { ICheckbox, ICustomizationSettings, IEmailNotificationSettings,IUser } from "../interfaces";
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

    public showBulkEdit = false;
    public availableClaims: ICheckbox[];
    public bulkMovieLimit?: number;
    public bulkEpisodeLimit?: number;

    constructor(private readonly identityService: IdentityService,
                private readonly settingsService: SettingsService,
                private readonly notificationService: NotificationService) { }

    public ngOnInit() {
        this.users = [];
        this.identityService.getUsers().subscribe(x => {
            this.users = x;
        });

        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
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

    public hasChecked(): boolean {
        return this.users.some(x => {
            return x.checked;
        });
    }

    public bulkUpdate() {
        const anyRoles = this.availableClaims.some(x => {
            return x.enabled;
        });

        this.users.forEach(x => {
            if(!x.checked) {
                return;
            }
            if(anyRoles) {
                x.claims = this.availableClaims;
            }
            if(this.bulkEpisodeLimit && this.bulkEpisodeLimit > 0) {
                x.episodeRequestLimit = this.bulkEpisodeLimit;
            }
            if(this.bulkMovieLimit && this.bulkMovieLimit > 0) {
                x.movieRequestLimit = this.bulkMovieLimit;
            }
            this.identityService.updateUser(x).subscribe(y => {
                if(!y.successful) {
                    this.notificationService.error(`Could not update user ${x.userName}. Reason ${y.errors[0]}`);
                }
            });
        });
        
        this.notificationService.success(`Updated users`);
        this.showBulkEdit = false;
        this.bulkMovieLimit = undefined;
        this.bulkEpisodeLimit = undefined;
    }
    
    public setOrder(value: string) {
        if (this.order === value) {
          this.reverse = !this.reverse;
        }
    
        this.order = value;
      }
}
