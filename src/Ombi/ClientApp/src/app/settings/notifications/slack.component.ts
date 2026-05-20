import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { ISlackNotificationSettings } from "../../interfaces";
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
    templateUrl: "./slack.component.html",
})
export class SlackComponent extends NotificationBaseComponent<ISlackNotificationSettings> {

    protected readonly providerName = "Slack";

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected override get testSuccessMessage(): string {
        return "Successfully sent a Slack message, please check the slack channel";
    }

    protected loadSettings(): Observable<ISlackNotificationSettings> {
        return this.settingsService.getSlackNotificationSettings();
    }

    protected saveSettings(settings: ISlackNotificationSettings): Observable<boolean> {
        return this.settingsService.saveSlackNotificationSettings(settings);
    }

    protected testSettings(settings: ISlackNotificationSettings): Observable<boolean> {
        return this.testerService.slackTest(settings);
    }

    protected buildForm(x: ISlackNotificationSettings) {
        return {
            enabled: [x.enabled],
            username: [x.username],
            webhookUrl: [x.webhookUrl, [Validators.required]],
            iconEmoji: [x.iconEmoji],
            iconUrl: [x.iconUrl],
            channel: [x.channel],
        };
    }

    protected override validate(settings: ISlackNotificationSettings): string | null {
        if (settings.iconEmoji && settings.iconUrl) {
            return "You cannot set both an emoji icon and a URL icon";
        }
        return null;
    }
}
