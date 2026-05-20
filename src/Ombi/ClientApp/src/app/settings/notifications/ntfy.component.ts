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

import { INtfyNotificationSettings } from "../../interfaces";
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
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationShellComponent,
    ],
    templateUrl: "./ntfy.component.html",
})
export class NtfyComponent extends NotificationBaseComponent<INtfyNotificationSettings> {

    protected readonly providerName = "Ntfy";

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected loadSettings(): Observable<INtfyNotificationSettings> {
        return this.settingsService.getNtfyNotificationSettings();
    }

    protected saveSettings(settings: INtfyNotificationSettings): Observable<boolean> {
        return this.settingsService.saveNtfyNotificationSettings(settings);
    }

    protected testSettings(settings: INtfyNotificationSettings): Observable<boolean> {
        return this.testerService.ntfyTest(settings);
    }

    protected buildForm(x: INtfyNotificationSettings) {
        return {
            enabled: [x.enabled],
            baseUrl: [x.baseUrl, [Validators.required]],
            topic: [x.topic, [Validators.required]],
            authorizationHeader: [x.authorizationHeader],
            priority: [x.priority],
        };
    }
}
