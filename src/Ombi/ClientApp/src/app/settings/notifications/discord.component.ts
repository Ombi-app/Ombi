import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { IDiscordNotifcationSettings } from "../../interfaces";
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
    templateUrl: "./discord.component.html",
})
export class DiscordComponent extends NotificationBaseComponent<IDiscordNotifcationSettings> {

    protected readonly providerName = "Discord";

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected override get testSuccessMessage(): string {
        return "Successfully sent a Discord message, please check the discord channel";
    }

    protected loadSettings(): Observable<IDiscordNotifcationSettings> {
        return this.settingsService.getDiscordNotificationSettings();
    }

    protected saveSettings(settings: IDiscordNotifcationSettings): Observable<boolean> {
        return this.settingsService.saveDiscordNotificationSettings(settings);
    }

    protected testSettings(settings: IDiscordNotifcationSettings): Observable<boolean> {
        return this.testerService.discordTest(settings);
    }

    protected buildForm(x: IDiscordNotifcationSettings) {
        return {
            enabled: [x.enabled],
            username: [x.username],
            webhookUrl: [x.webhookUrl, [Validators.required]],
            icon: [x.icon],
            hideUser: [x.hideUser],
        };
    }
}
