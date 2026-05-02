import { Component, OnDestroy, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import { INotificationTemplates, ITwilioSettings, IWhatsAppSettings } from "../../../interfaces";
import { NotificationService, SettingsService, TesterService } from "../../../services";
import { NotificationShellComponent } from "../shared/notification-shell.component";

@Component({
    standalone: true,
    selector: "app-whatsapp",
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
    templateUrl: "./whatsapp.component.html",
})
export class WhatsAppComponent implements OnInit, OnDestroy {

    public form!: UntypedFormGroup;
    public templates: INotificationTemplates[] = [];
    public loading = true;

    private originalSettings!: ITwilioSettings;
    private readonly destroy$ = new Subject<void>();

    constructor(private readonly settingsService: SettingsService,
                private readonly notificationService: NotificationService,
                private readonly fb: UntypedFormBuilder,
                private readonly testerService: TesterService) { }

    public ngOnInit(): void {
        this.settingsService.getTwilioSettings()
            .pipe(takeUntil(this.destroy$))
            .subscribe(twilio => {
                this.originalSettings = twilio;
                const whatsapp = twilio.whatsAppSettings;
                this.templates = whatsapp?.notificationTemplates ?? [];
                this.form = this.fb.group({
                    enabled: [!!whatsapp?.enabled],
                    from: [whatsapp?.from ?? "", [Validators.required]],
                    accountSid: [whatsapp?.accountSid ?? "", [Validators.required]],
                    authToken: [whatsapp?.authToken ?? "", [Validators.required]],
                });
                this.loading = false;
            });
    }

    public ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    public onSubmit(form: UntypedFormGroup): void {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const wrapper: ITwilioSettings = {
            ...this.originalSettings,
            whatsAppSettings: this.toWhatsApp(form),
        };
        this.settingsService.saveTwilioSettings(wrapper)
            .pipe(takeUntil(this.destroy$))
            .subscribe(ok => {
                if (ok) {
                    this.notificationService.success("Successfully saved the WhatsApp settings");
                } else {
                    this.notificationService.error("There was an error when saving the WhatsApp settings");
                }
            });
    }

    public test(form: UntypedFormGroup): void {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        this.testerService.whatsAppTest(this.toWhatsApp(form))
            .pipe(takeUntil(this.destroy$))
            .subscribe(ok => {
                if (ok) {
                    this.notificationService.success(
                        "Successfully sent a WhatsApp message, please check the appropriate channel");
                } else {
                    this.notificationService.error(
                        "There was an error when sending the WhatsApp message. Please check your settings");
                }
            });
    }

    private toWhatsApp(form: UntypedFormGroup): IWhatsAppSettings {
        const value = form.value;
        return {
            enabled: value.enabled ? 1 : 0,
            from: value.from,
            accountSid: value.accountSid,
            authToken: value.authToken,
            notificationTemplates: this.templates,
        };
    }
}
