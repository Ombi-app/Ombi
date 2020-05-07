import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { IMobileNotifcationSettings, IMobileUsersViewModel, INotificationTemplates, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { MobileService, SettingsService } from "../../services";

@Component({
    templateUrl: "./mobile.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class MobileComponent implements OnInit {

    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;
    public userList: IMobileUsersViewModel[];
    public testUserId: string;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private testerService: TesterService,
                private mobileService: MobileService) { }

    public ngOnInit() {
        this.settingsService.getMobileNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
            });
        });

        this.mobileService.getUserDeviceList().subscribe(x => {
            if (x.length <= 0) {
                this.userList = [];
                this.userList.push({username: "None", devices: 0, userId: ""});
            } else {
                this.userList = x;
            }
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IMobileNotifcationSettings> form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveMobileNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Mobile settings");
            } else {
                this.notificationService.success("There was an error when saving the Mobile settings");
            }
        });

    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        if (!this.testUserId) {
            this.notificationService.warning("Warning", "Please select a user to send the test notification");
            return;
        }

        this.testerService.mobileNotificationTest({settings: form.value, userId: this.testUserId}).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Mobile message, please check the admin mobile device");
            } else {
                this.notificationService.error("There was an error when sending the Mobile message. Please check your settings");
            }
        });

    }

    public remove() {
        if (!this.testUserId) {
            this.notificationService.warning("Warning", "Please select a user to remove");
            return;
        }

        this.mobileService.deleteUser(this.testUserId).subscribe(x => {
            if (x) {
                this.notificationService.success("Removed users notification");
                const userToRemove = this.userList.filter(u => {
                    return u.userId === this.testUserId;
                })[1];
                this.userList.splice(this.userList.indexOf(userToRemove),1);
            } else {
                this.notificationService.error("There was an error when removing the notification. Please check your logs");
            }
        });

    }
}
