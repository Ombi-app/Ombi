import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { IdentityService } from '../services/identity.service';
import { NotificationService } from '../services/notification.service';
import { SettingsService } from '../services/settings.service';
import { ICustomizationSettings } from '../interfaces/ISettings';

@Component({
    templateUrl: './resetpassword.component.html',
    styleUrls: ['./login.component.scss']
})
export class ResetPasswordComponent implements OnInit {


    constructor(private identityService: IdentityService, private notify: NotificationService,
        private fb: FormBuilder, private settingsService: SettingsService) {
        this.form = this.fb.group({
            email: ["", [Validators.required]],
        });
    }

    form: FormGroup;
    customizationSettings: ICustomizationSettings;

    ngOnInit(): void {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
    }


    onSubmit(form: FormGroup): void {
        if (form.invalid) {
            this.notify.error("Validation", "Email address is required");
            return
        }
        this.identityService.submitResetPassword(form.value.email).subscribe(x => {
            x.errors.forEach((val) => {
                this.notify.success("Password Reset", val);
            });
        });
    }
}