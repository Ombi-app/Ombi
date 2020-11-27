import { Component, OnInit } from "@angular/core";

import { IMassEmailModel, IMassEmailUserModel } from "../../interfaces";
import { IdentityService, NotificationMessageService, NotificationService, SettingsService } from "../../services";

@Component({
    templateUrl: "./massemail.component.html",
    styleUrls: ["./massemail.component.scss"]
})
export class MassEmailComponent implements OnInit {

    public users: IMassEmailUserModel[] = [];
    public message: string;
    public subject: string;

    public missingSubject = false;

    public emailEnabled: boolean;

    constructor(private readonly notification: NotificationService,
                private readonly identityService: IdentityService,
                private readonly notificationMessageService: NotificationMessageService,
                private readonly settingsService: SettingsService) {
    }

    public ngOnInit(): void {
       this.identityService.getUsers().subscribe(x => {
        x.forEach(u => {
            this.users.push({
                user: u,
                selected: false,
            });
        });
       });
       this.settingsService.getEmailSettingsEnabled().subscribe(x => this.emailEnabled = x);
    }

    public selectAllUsers() {
        this.users.forEach(u => u.selected = !u.selected);
    }

    public send() {
        if(!this.subject) {
            this.missingSubject = true;
            return;
        }
        if(!this.emailEnabled) {
            this.notification.error("You have not yet setup your email notifications, do that first!");
            return;
        }
        this.missingSubject = false;
        // Where(x => x.selected).Select(x => x.user)
        const selectedUsers = this.users.filter(u => {
            return u.selected;
        }).map(u => u.user);

        if(selectedUsers.length <=0) {
            this.notification.error("You need to select at least one user to send the email");
            return;
        }

        const model = <IMassEmailModel>{
            users: selectedUsers,
            subject: this.subject,
            body: this.message,
        };
        this.notification.info("Sending","Sending mass email... Please wait");
        this.notificationMessageService.sendMassEmail(model).subscribe(x => {
            this.notification.success("We have sent the mass email to the users selected!");
        });
    }
}
