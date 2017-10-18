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

    constructor(private identityService: IdentityService, private notify: NotificationService,
                private fb: FormBuilder, private settingsService: SettingsService) {
        this.form = this.fb.group({
            email: ["", [Validators.required]],
        });
    }

    public ngOnInit() {
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
