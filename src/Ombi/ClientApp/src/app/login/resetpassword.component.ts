import { APP_BASE_HREF, CommonModule } from "@angular/common";
import { Component, OnInit, Inject } from "@angular/core";
import { FormsModule, ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { fadeInOutAnimation } from "../animations/fadeinout";

import { ICustomizationSettings } from "../interfaces";
import { IdentityService, NotificationService, SettingsService } from "../services";
import { CustomizationFacade } from "../state/customization";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { ImageBackgroundComponent } from "../components";
import { MatCardModule } from "@angular/material/card";
import { TranslateModule } from "@ngx-translate/core";

@Component({
        standalone: true,
    templateUrl: "./resetpassword.component.html",
    styleUrls: ["./login.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatCardModule,
        TranslateModule,
        ImageBackgroundComponent
    ]
})
export class ResetPasswordComponent implements OnInit {

    public form: UntypedFormGroup;
    public customizationSettings: ICustomizationSettings;
    public emailSettingsEnabled: boolean;
    public baseUrl: string;
    private href: string;

    constructor(private identityService: IdentityService, private notify: NotificationService,
                private fb: UntypedFormBuilder, private settingsService: SettingsService, @Inject(APP_BASE_HREF) href:string,
                private customizationFacade: CustomizationFacade) {
                    this.href = href;
        this.form = this.fb.group({
            email: ["", [Validators.required]],
        });
    }

    public ngOnInit() {

        const base = this.href;
        if (base.length > 1) {
            this.baseUrl = base;
        }
        this.customizationFacade.settings$().subscribe(x => this.customizationSettings = x);
        this.settingsService.getEmailSettingsEnabled().subscribe(x => this.emailSettingsEnabled = x);
    }

    public onSubmit(form: UntypedFormGroup) {
        if (this.emailSettingsEnabled) {

            if (form.invalid) {
                this.notify.error("Email address is required");
                return;
            }
            this.identityService.submitResetPassword(form.value.email).subscribe(x => {
                x.errors.forEach((val) => {
                    this.notify.success(val);
                });
            });
        } else {
            this.notify.error("Sorry but the administrator has not set up email notifications!");
            return;
        }
    }
}
