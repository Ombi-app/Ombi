import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable } from "rxjs";

import { IPushoverNotificationSettings } from "../../interfaces";
import { NotificationService, SettingsService, TesterService } from "../../services";
import { NotificationBaseComponent } from "./shared/notification-base.component";
import { NotificationShellComponent } from "./shared/notification-shell.component";

interface PushoverSound { value: string; label: string; }

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationShellComponent,
    ],
    templateUrl: "./pushover.component.html",
})
export class PushoverComponent extends NotificationBaseComponent<IPushoverNotificationSettings> {

    protected readonly providerName = "Pushover";

    public readonly sounds: ReadonlyArray<PushoverSound> = [
        { value: "pushover", label: "Pushover" },
        { value: "bike", label: "Bike" },
        { value: "bugle", label: "Bugle" },
        { value: "cashregister", label: "Cash Register" },
        { value: "classical", label: "Classical" },
        { value: "cosmic", label: "Cosmic" },
        { value: "falling", label: "Falling" },
        { value: "gamelan", label: "Gamelan" },
        { value: "incoming", label: "Incoming" },
        { value: "intermission", label: "Intermission" },
        { value: "magic", label: "Magic" },
        { value: "mechanical", label: "Mechanical" },
        { value: "pianobar", label: "Piano Bar" },
        { value: "siren", label: "Siren" },
        { value: "spacealarm", label: "Space Alarm" },
        { value: "tugboat", label: "Tug Boat" },
        { value: "alien", label: "Alien Alarm (long)" },
        { value: "climb", label: "Climb (long)" },
        { value: "persistent", label: "Persistent (long)" },
        { value: "echo", label: "Pushover Echo (long)" },
        { value: "updown", label: "Up Down (long)" },
        { value: "none", label: "None" },
    ];

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected loadSettings(): Observable<IPushoverNotificationSettings> {
        return this.settingsService.getPushoverNotificationSettings();
    }

    protected saveSettings(settings: IPushoverNotificationSettings): Observable<boolean> {
        return this.settingsService.savePushoverNotificationSettings(settings);
    }

    protected testSettings(settings: IPushoverNotificationSettings): Observable<boolean> {
        return this.testerService.pushoverTest(settings);
    }

    protected buildForm(x: IPushoverNotificationSettings) {
        return {
            enabled: [x.enabled],
            userToken: [x.userToken],
            accessToken: [x.accessToken, [Validators.required]],
            priority: [x.priority],
            sound: [x.sound],
        };
    }
}
