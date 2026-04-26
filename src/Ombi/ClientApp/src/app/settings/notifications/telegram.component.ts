import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatRadioModule } from "@angular/material/radio";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { ITelegramNotifcationSettings } from "../../interfaces";
import { NotificationService, SettingsService, TesterService } from "../../services";
import { NotificationBaseComponent } from "./shared/notification-base.component";
import { NotificationShellComponent } from "./shared/notification-shell.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatRadioModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationShellComponent,
    ],
    templateUrl: "./telegram.component.html",
})
export class TelegramComponent extends NotificationBaseComponent<ITelegramNotifcationSettings> {

    protected readonly providerName = "Telegram";

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected override get testSuccessMessage(): string {
        return "Successfully sent a Telegram message, please check the Telegram channel";
    }

    protected loadSettings(): Observable<ITelegramNotifcationSettings> {
        return this.settingsService.getTelegramNotificationSettings();
    }

    protected saveSettings(settings: ITelegramNotifcationSettings): Observable<boolean> {
        return this.settingsService.saveTelegramNotificationSettings(settings);
    }

    protected testSettings(settings: ITelegramNotifcationSettings): Observable<boolean> {
        return this.testerService.telegramTest(settings);
    }

    protected buildForm(x: ITelegramNotifcationSettings) {
        return {
            enabled: [x.enabled],
            botApi: [x.botApi, [Validators.required]],
            chatId: [x.chatId, [Validators.required]],
            parseMode: [x.parseMode, [Validators.required]],
        };
    }
}
