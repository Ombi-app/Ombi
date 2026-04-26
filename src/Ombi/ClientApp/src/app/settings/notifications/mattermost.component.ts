import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { IMattermostNotifcationSettings } from "../../interfaces";
import { NotificationService, SettingsService, TesterService } from "../../services";
import { NotificationBaseComponent } from "./shared/notification-base.component";
import { NotificationShellComponent } from "./shared/notification-shell.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationShellComponent,
    ],
    templateUrl: "./mattermost.component.html",
})
export class MattermostComponent extends NotificationBaseComponent<IMattermostNotifcationSettings> {

    protected readonly providerName = "Mattermost";

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected override get testSuccessMessage(): string {
        return "Successfully sent a Mattermost message, please check the appropriate channel";
    }

    protected loadSettings(): Observable<IMattermostNotifcationSettings> {
        return this.settingsService.getMattermostNotificationSettings();
    }

    protected saveSettings(settings: IMattermostNotifcationSettings): Observable<boolean> {
        return this.settingsService.saveMattermostNotificationSettings(settings);
    }

    protected testSettings(settings: IMattermostNotifcationSettings): Observable<boolean> {
        return this.testerService.mattermostTest(settings);
    }

    protected buildForm(x: IMattermostNotifcationSettings) {
        return {
            enabled: [x.enabled],
            username: [x.username],
            webhookUrl: [x.webhookUrl, [Validators.required]],
            channel: [x.channel],
            iconUrl: [x.iconUrl],
        };
    }
}
