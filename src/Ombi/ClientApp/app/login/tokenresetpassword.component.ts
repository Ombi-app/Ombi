import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { ActivatedRoute, Params } from "@angular/router";

import { ICustomizationSettings } from "../interfaces";
import { IResetPasswordToken } from "../interfaces";
import { IdentityService } from "../services";
import { NotificationService } from "../services";
import { SettingsService } from "../services";

@Component({
    templateUrl: "./tokenresetpassword.component.html",
    styleUrls: ["./login.component.scss"],
})
export class TokenResetPasswordComponent implements OnInit {

    public form: FormGroup;
    public customizationSettings: ICustomizationSettings;

    constructor(private identityService: IdentityService, private router: Router, private route: ActivatedRoute, private notify: NotificationService,
                private fb: FormBuilder, private settingsService: SettingsService) {

        this.route.queryParams
            .subscribe((params: Params) => {
                this.form = this.fb.group({
                    email: ["", [Validators.required]],
                    password: ["", [Validators.required]],
                    confirmPassword: ["", [Validators.required]],
                    token: [params.token],
                });
            });
    }

    public ngOnInit() {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notify.error("Validation", "Email address is required");
            return;
        }
        const token = form.value as IResetPasswordToken;
        this.identityService.resetPassword(token).subscribe(x => {
            if (x.successful) {
                this.notify.success("Success", `Your Password has been reset`);
                this.router.navigate(["login"]);
            } else {
                x.errors.forEach((val) => {
                    this.notify.error("Error", val);
                });
            }
        });

    }
}
