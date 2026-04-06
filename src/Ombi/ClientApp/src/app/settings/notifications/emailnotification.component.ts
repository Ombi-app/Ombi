import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators, ReactiveFormsModule, FormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { IEmailNotificationSettings, INotificationTemplates, NotificationType } from "../../interfaces";
import { TesterService } from "../../services";
import { ValidationService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";
import { NotificationTemplate } from "./notificationtemplate.component";
import { SettingsMenuComponent } from "../settingsmenu.component";

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
        NotificationTemplate,
        SettingsMenuComponent
    ],
    templateUrl: "./emailnotification.component.html",
    styleUrls: ["./notificationtemplate.component.scss"]
})
export class EmailNotificationComponent implements OnInit {
    public NotificationType = NotificationType;
    public templates: INotificationTemplates[];
    public emailForm: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: UntypedFormBuilder,
                private validationService: ValidationService,
                private testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getEmailNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.emailForm = this.fb.group({
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
                useBasicWrapper: [x.useBasicWrapper ?? true],
                includeWrapperLogo: [x.includeWrapperLogo ?? true],
                includeWrapperPoster: [x.includeWrapperPoster ?? true],
            });

            if (x.authentication) {
                this.validationService.enableValidation(this.emailForm, "username");
                this.validationService.enableValidation(this.emailForm, "password");
            }

            this.subscribeToAuthChanges();
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = <IEmailNotificationSettings> form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveEmailNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Email settings");
            } else {
                this.notificationService.success("There was an error when saving the Email settings");
            }
        });

    }

    public test(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.testerService.emailTest(form.value).subscribe(x => {
              if (x === true) {
                this.notificationService.success("Successfully sent an email message, please check your inbox");
            } else {
                this.notificationService.error("There was an error when sending the Email message, please check your settings.");
            }
        });
    }

    private subscribeToAuthChanges() {
        const authCtrl = this.emailForm.controls.authentication;
        const changes$ = authCtrl.valueChanges;

        changes$.subscribe((auth: boolean) => {

            if (auth) {
                this.validationService.enableValidation(this.emailForm, "username");
                this.validationService.enableValidation(this.emailForm, "password");
            } else {
                this.validationService.disableValidation(this.emailForm, "username");
                this.validationService.disableValidation(this.emailForm, "password");
            }
        });
    }
}
