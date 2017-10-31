﻿import { PlatformLocation } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { DomSanitizer } from "@angular/platform-browser";

import { ICustomizationSettings } from "../interfaces";
import { IdentityService, ImageService,NotificationService, SettingsService } from "../services";

@Component({
    templateUrl: "./resetpassword.component.html",
    styleUrls: ["./login.component.scss"],
})
export class ResetPasswordComponent implements OnInit {

    public form: FormGroup;
    public customizationSettings: ICustomizationSettings;
    public emailSettingsEnabled: boolean;
    public baseUrl: string;
    public background: any;

    constructor(private identityService: IdentityService, private notify: NotificationService,
                private fb: FormBuilder, private settingsService: SettingsService, private location: PlatformLocation,
                private images: ImageService, private sanitizer: DomSanitizer) {
        this.form = this.fb.group({
            email: ["", [Validators.required]],
        });
    }

    public ngOnInit() {
        this.images.getRandomBackground().subscribe(x => {
            this.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%),url(" + x.url + ")");
        });
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getEmailSettingsEnabled().subscribe(x => this.emailSettingsEnabled = x);
    }

    public onSubmit(form: FormGroup) {
        if (this.emailSettingsEnabled) {

            if (form.invalid) {
                this.notify.error("Email address is required");
                return;
            }
            this.identityService.submitResetPassword(form.value.email).subscribe(x => {
                x.errors.forEach((val) => {
                    this.notify.success("Password Reset", val);
                });
            });
        } else {
            this.notify.error("Sorry but the administrator has not set up email notifications!");
            return;
        }
    }
}
