import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder } from "@angular/forms";
import { SelectionModel } from "@angular/cdk/collections";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { ICloudMobileModel, IMobileNotifcationSettings } from "../../interfaces";
import { NotificationService, SettingsService, TesterService } from "../../services";
import { CloudMobileService } from "../../services/cloudmobile.service";
import { NotificationBaseComponent } from "./shared/notification-base.component";
import { NotificationShellComponent } from "./shared/notification-shell.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCardModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTableModule,
        MatTooltipModule,
        TranslateModule,
        NotificationShellComponent,
    ],
    providers: [CloudMobileService],
    templateUrl: "./cloudmobile.component.html",
    styleUrls: ["./cloudmobile.component.scss"],
})
export class CloudMobileComponent extends NotificationBaseComponent<IMobileNotifcationSettings> {

    protected readonly providerName = "Mobile";

    public devices = new MatTableDataSource<ICloudMobileModel>([]);
    public selection = new SelectionModel<ICloudMobileModel>(true, []);
    public displayedColumns: string[] = ["select", "username", "deviceCount"];
    public message = "";
    public sending = false;

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService,
                private readonly mobileService: CloudMobileService) {
        super(settingsService, notificationService, fb, testerService);
        this.mobileService.getDevices().subscribe(result => {
            this.devices.data = result ?? [];
        });
    }

    protected loadSettings(): Observable<IMobileNotifcationSettings> {
        return this.settingsService.getMobileNotificationSettings();
    }

    protected saveSettings(settings: IMobileNotifcationSettings): Observable<boolean> {
        return this.settingsService.saveMobileNotificationSettings(settings);
    }

    protected testSettings(_settings: IMobileNotifcationSettings): Observable<boolean> {
        return new Observable<boolean>(sub => { sub.next(false); sub.complete(); });
    }

    protected buildForm(x: IMobileNotifcationSettings) {
        return {
            enabled: [x.enabled],
        };
    }

    public toggleAll(): void {
        if (this.allSelected()) {
            this.selection.clear();
        } else {
            this.devices.data.forEach(row => this.selection.select(row));
        }
    }

    public allSelected(): boolean {
        return this.devices.data.length > 0
            && this.selection.selected.length === this.devices.data.length;
    }

    public someSelected(): boolean {
        return this.selection.selected.length > 0 && !this.allSelected();
    }

    public deviceCount(row: ICloudMobileModel): number {
        return row.devices?.length ?? 0;
    }

    public async sendBroadcast(): Promise<void> {
        if (!this.selection.selected.length) {
            this.notificationService.warning("Warning", "Please select at least one user to send the notification");
            return;
        }
        if (!this.message?.trim()) {
            this.notificationService.error("Please enter a message before sending");
            return;
        }

        this.sending = true;
        try {
            await Promise.all(
                this.selection.selected.map(user => this.mobileService.send(user.userId, this.message)),
            );
            this.notificationService.success("Mobile notification sent to the selected users");
        } finally {
            this.sending = false;
        }
    }
}
