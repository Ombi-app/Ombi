import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { Observable, Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

import { IEmailNotificationSettings } from "../../interfaces";
import { NotificationService, SettingsService, TesterService, ValidationService } from "../../services";
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
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule,
        NotificationShellComponent,
    ],
    templateUrl: "./emailnotification.component.html",
})
export class EmailNotificationComponent extends NotificationBaseComponent<IEmailNotificationSettings> {

    protected readonly providerName = "Email";
    private readonly authChanges$ = new Subject<void>();

    constructor(settingsService: SettingsService,
                notificationService: NotificationService,
                fb: UntypedFormBuilder,
                private readonly validationService: ValidationService,
                testerService: TesterService) {
        super(settingsService, notificationService, fb, testerService);
    }

    public get emailForm(): UntypedFormGroup { return this.form; }

    protected override get savedMessage(): string { return "Successfully saved Email settings"; }
    protected override get saveErrorMessage(): string {
        return "There was an error when saving the Email settings";
    }
    protected override get testSuccessMessage(): string {
        return "Successfully sent an email message, please check your inbox";
    }
    protected override get testErrorMessage(): string {
        return "There was an error when sending the Email message, please check your settings.";
    }

    protected loadSettings(): Observable<IEmailNotificationSettings> {
        return this.settingsService.getEmailNotificationSettings();
    }

    protected saveSettings(settings: IEmailNotificationSettings): Observable<boolean> {
        return this.settingsService.saveEmailNotificationSettings(settings);
    }

    protected testSettings(settings: IEmailNotificationSettings): Observable<boolean> {
        return this.testerService.emailTest(settings);
    }

    protected buildForm(x: IEmailNotificationSettings) {
        return {
            enabled: [x.enabled],
            authentication: [x.authentication],
            host: [x.host, [Validators.required]],
            password: [x.password],
            port: [x.port, [Validators.required]],
            senderAddress: [x.senderAddress, [Validators.required, Validators.email]],
            senderName: [x.senderName],
            username: [x.username],
            disableTLS: [x.disableTLS],
            disableCertificateChecking: [x.disableCertificateChecking],
        };
    }

    protected override onFormReady(form: UntypedFormGroup, settings: IEmailNotificationSettings): void {
        if (settings.authentication) {
            this.applyAuthValidators(true);
        }
        form.controls.authentication.valueChanges
            .pipe(takeUntil(this.authChanges$))
            .subscribe((auth: boolean) => this.applyAuthValidators(auth));
    }

    public override ngOnDestroy(): void {
        this.authChanges$.next();
        this.authChanges$.complete();
        super.ngOnDestroy();
    }

    private applyAuthValidators(authEnabled: boolean): void {
        if (authEnabled) {
            this.validationService.enableValidation(this.form, "username");
            this.validationService.enableValidation(this.form, "password");
        } else {
            this.validationService.disableValidation(this.form, "username");
            this.validationService.disableValidation(this.form, "password");
        }
    }
}
