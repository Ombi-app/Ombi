import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { IWebhookNotificationSettings } from "../../interfaces";
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
    templateUrl: "./webhook.component.html",
})
export class WebhookComponent extends NotificationBaseComponent<IWebhookNotificationSettings> {

    protected readonly providerName = "Webhook";
    protected override attachTemplates = false;

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected loadSettings(): Observable<IWebhookNotificationSettings> {
        return this.settingsService.getWebhookNotificationSettings();
    }

    protected saveSettings(settings: IWebhookNotificationSettings): Observable<boolean> {
        return this.settingsService.saveWebhookNotificationSettings(settings);
    }

    protected testSettings(settings: IWebhookNotificationSettings): Observable<boolean> {
        return this.testerService.webhookTest(settings);
    }

    protected buildForm(x: IWebhookNotificationSettings) {
        return {
            enabled: [x.enabled],
            webhookUrl: [x.webhookUrl, [Validators.required]],
            applicationToken: [x.applicationToken],
        };
    }
}
