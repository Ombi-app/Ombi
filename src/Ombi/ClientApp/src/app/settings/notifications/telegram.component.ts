import { Component, OnInit } from "@angular/core";
import { ReactiveFormsModule, FormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { INotificationTemplates, ITelegramNotifcationSettings, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { RadioButtonModule } from "primeng/radiobutton";
import { NotificationTemplate } from "./notificationtemplate.component";
import { SettingsMenuComponent } from "../settingsmenu.component";
import { WikiComponent } from "../wiki.component";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        RadioButtonModule,
        NotificationTemplate,
        SettingsMenuComponent,
        WikiComponent
    ],
    templateUrl: "./telegram.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class TelegramComponent implements OnInit {

    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public form: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: UntypedFormBuilder,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getTelegramNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.form = this.fb.group({
                enabled: [x.enabled],
                botApi: [x.botApi, [Validators.required]],
                chatId: [x.chatId, [Validators.required]],
                parseMode: [x.parseMode, [Validators.required]],

            });
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <ITelegramNotifcationSettings> form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveTelegramNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Telegram settings");
            } else {
                this.notificationService.success("There was an error when saving the Telegram settings");
            }
        });

    }

    public test(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.telegramTest(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully sent a Telegram message, please check the Telegram channel");
            } else {
                this.notificationService.error("There was an error when sending the Telegram message. Please check your settings");
            }
        });

    }
}
