import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { IMobileNotifcationSettings, IMobileUsersViewModel, INotificationTemplates, NotificationType, ICloudMobileDevices, ICloudMobileModel } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { MobileService, SettingsService } from "../../services";
import { CloudMobileService } from "../../services/cloudmobile.service";
import { SelectionModel } from "@angular/cdk/collections";
import { MatTableDataSource } from "@angular/material/table";

@Component({
    templateUrl: "./cloudmobile.component.html",
})
export class CloudMobileComponent implements OnInit {

    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: FormGroup;
    public devices: MatTableDataSource<ICloudMobileModel>;
    public selection = new SelectionModel<ICloudMobileModel>(true, []);
    displayedColumns: string[] = ['select', 'username'];
    public message: string;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder,
                private mobileService: CloudMobileService) { }

    public async ngOnInit() {
        this.settingsService.getMobileNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
            });
        });

        var result = await this.mobileService.getDevices().toPromise();
            if (result.length > 0) {
                this.devices = new MatTableDataSource(result);
            }
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

    public async sendMessage(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        if (this.selection.selected.length <= 0) {
            this.notificationService.warning("Warning", "Please select a user to send the test notification");
            return;
        }

        await this.selection.selected.forEach(async (u) => {
            await this.mobileService.send(u.userId, this.message);
           
            this.notificationService.success(
                "Successfully sent a Mobile message");
            
            
        });
    }
}
