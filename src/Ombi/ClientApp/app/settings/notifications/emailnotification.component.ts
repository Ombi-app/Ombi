import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { INotificationTemplates, IEmailNotificationSettings, NotificationType } from '../../interfaces/INotifcationSettings';
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";
import { ValidationService } from "../../services/helpers/validation.service";

@Component({
    templateUrl: './emailnotification.component.html',
})
export class EmailNotificationComponent implements OnInit {
    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private fb: FormBuilder,
        private validationService: ValidationService) { }

    NotificationType = NotificationType;
    templates: INotificationTemplates[];

    emailForm: FormGroup;

    ngOnInit(): void {
        this.settingsService.getEmailNotificationSettings().subscribe(x => {
            this.templates = x.notificationTemplates;

            this.emailForm = this.fb.group({
                enabled: [x.enabled],
                authentication: [x.authentication],
                host: [x.host, [Validators.required]],
                password: [x.password],
                port: [x.port, [Validators.required]],
                sender: [x.sender, [Validators.required, Validators.email]],
                username: [x.username],
                adminEmail: [x.adminEmail, [Validators.required, Validators.email]],
            });

            if (x.authentication) {
                this.validationService.enableValidation(this.emailForm, 'username');
                this.validationService.enableValidation(this.emailForm, 'password');
            }

            this.subscribeToAuthChanges();
        });
    }

    onSubmit(form: FormGroup) {
        console.log(form.value, form.valid);

        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        var settings = <IEmailNotificationSettings>form.value;
        settings.notificationTemplates = this.templates;

        this.settingsService.saveEmailNotificationSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Email settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Email settings");
            }
        });

    }

    save() {

    }

    private subscribeToAuthChanges() {
        const authCtrl = this.emailForm.controls.authentication;
        const changes$ = authCtrl.valueChanges;

        changes$.subscribe((auth: boolean) => {

            if (auth) {
                this.validationService.enableValidation(this.emailForm, 'username');
                this.validationService.enableValidation(this.emailForm, 'password');
            } else {
                this.validationService.disableValidation(this.emailForm, 'username');
                this.validationService.disableValidation(this.emailForm, 'password');
            }
        });
    }
}