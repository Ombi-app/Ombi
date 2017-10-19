import { PlatformLocation } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { ICustomizationSettings, IEmailNotificationSettings } from "../interfaces";
import { IdentityService } from "../services";
import { NotificationService } from "../services";
import { SettingsService } from "../services";

@Component({
    templateUrl: "./resetpassword.component.html",
    styleUrls: ["./login.component.scss"],
})
export class ResetPasswordComponent implements OnInit {

    public form: FormGroup;
    public customizationSettings: ICustomizationSettings;
    public emailSettings: IEmailNotificationSettings;
    public baseUrl: string;

    constructor(private identityService: IdentityService, private notify: NotificationService,
                private fb: FormBuilder, private settingsService: SettingsService, private location: PlatformLocation) {
        this.form = this.fb.group({
            email: ["", [Validators.required]],
        });
    }

    public ngOnInit() {
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getEmailNotificationSettings().subscribe(x => this.emailSettings = x);
    }

    public onSubmit(form: FormGroup) {
        if (this.emailSettings && this.emailSettings.enabled) {

            if (form.invalid) {
                this.notify.error("Validation", "Email address is required");
                return;
            }
            this.identityService.submitResetPassword(form.value.email).subscribe(x => {
                x.errors.forEach((val) => {
                    this.notify.success("Password Reset", val);
                });
            });
        } else {
            this.notify.error("Not Setup", "Sorry but the administrator has not set up email notitfications!");
            return;
        }
    }
}
