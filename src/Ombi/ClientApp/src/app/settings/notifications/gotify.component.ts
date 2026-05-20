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

import { IGotifyNotificationSettings } from "../../interfaces";
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
    templateUrl: "./gotify.component.html",
})
export class GotifyComponent extends NotificationBaseComponent<IGotifyNotificationSettings> {

    protected readonly providerName = "Gotify";

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    protected loadSettings(): Observable<IGotifyNotificationSettings> {
        return this.settingsService.getGotifyNotificationSettings();
    }

    protected saveSettings(settings: IGotifyNotificationSettings): Observable<boolean> {
        return this.settingsService.saveGotifyNotificationSettings(settings);
    }

    protected testSettings(settings: IGotifyNotificationSettings): Observable<boolean> {
        return this.testerService.gotifyTest(settings);
    }

    protected buildForm(x: IGotifyNotificationSettings) {
        return {
            enabled: [x.enabled],
            baseUrl: [x.baseUrl, [Validators.required]],
            applicationToken: [x.applicationToken, [Validators.required]],
            priority: [x.priority],
        };
    }
}
