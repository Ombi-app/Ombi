import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { IPushbulletNotificationSettings } from "../../interfaces";
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
    templateUrl: "./pushbullet.component.html",
})
export class PushbulletComponent extends NotificationBaseComponent<IPushbulletNotificationSettings> {

    protected readonly providerName = "Pushbullet";

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected loadSettings(): Observable<IPushbulletNotificationSettings> {
        return this.settingsService.getPushbulletNotificationSettings();
    }

    protected saveSettings(settings: IPushbulletNotificationSettings): Observable<boolean> {
        return this.settingsService.savePushbulletNotificationSettings(settings);
    }

    protected testSettings(settings: IPushbulletNotificationSettings): Observable<boolean> {
        return this.testerService.pushbulletTest(settings);
    }

    protected buildForm(x: IPushbulletNotificationSettings) {
        return {
            enabled: [x.enabled],
            channelTag: [x.channelTag],
            accessToken: [x.accessToken, [Validators.required]],
        };
    }
}
