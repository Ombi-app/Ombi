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

import { IMassEmailModel, IMassEmailUserModel } from "../../interfaces";
import { IdentityService, NotificationMessageService, NotificationService, SettingsService } from "../../services";
import { MatDividerModule } from "@angular/material/divider";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        FormsModule,
        MatDividerModule
    ],
    providers: [
        NotificationMessageService
    ],
    templateUrl: "./massemail.component.html",
    styleUrls: ["./massemail.component.scss"]
})
export class MassEmailComponent implements OnInit {

    public users: IMassEmailUserModel[] = [];
    public message: string;
    public subject: string;
    public bcc: boolean;

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
            if (u.emailAddress) {
                this.users.push({
                    user: u,
                    selected: false,
                });
            }
        });
       });
       this.settingsService.getEmailSettingsEnabled().subscribe(x => this.emailEnabled = x);
    }

    public selectAllUsers(event: any) {
        this.users.forEach(u => u.selected = event.checked);
    }

    public send() {
        if(!this.subject) {
            this.missingSubject = true;
            return;
        }
        // if(!this.emailEnabled) {
        //     this.notification.error("You have not yet setup your email notifications, do that first!");
        //     return;
        // }
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
            bcc: this.bcc,
        };
        this.notification.info("Sending","Sending mass email... Please wait");
        this.notificationMessageService.sendMassEmail(model).subscribe(x => {
            this.notification.success("We have sent the mass email to the users selected!");
        });
    }
}
