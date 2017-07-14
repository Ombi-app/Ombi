import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { IdentityService } from '../services/identity.service';
import { NotificationService } from '../services/notification.service';
import { SettingsService } from '../services/settings.service';
import { ICustomizationSettings } from '../interfaces/ISettings';

@Component({
    templateUrl: './tokenresetpassword.component.html',
    styleUrls: ['./login.component.scss']
})
export class TokenResetPasswordComponent implements OnInit {
    constructor(private identityService: IdentityService, private router: Router, private route: ActivatedRoute, private notify: NotificationService,
        private fb: FormBuilder, private settingsService: SettingsService) {

        this.route.params
            .subscribe(params => {
                this.form = this.fb.group({
                    email: ["", [Validators.required]],
                    password: ["", [Validators.required]],
                    confirmPassword: ["", [Validators.required]],
                    token: [params['token']]
                });
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

        this.identityService.resetPassword(form.value).subscribe(x => {
            if (x.successful) {
                this.notify.success("Success", `Your Password has been reset`)
                this.router.navigate(['login']);
            } else {
                x.errors.forEach((val) => {
                    this.notify.error("Error", val);
                });
            }
        });

    }
}