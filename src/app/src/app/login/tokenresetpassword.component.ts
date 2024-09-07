import { ActivatedRoute, Params } from "@angular/router";
import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { IdentityService } from "../services";

import { CustomizationFacade } from "../state/customization";
import { ICustomizationSettings } from "../interfaces";
import { IResetPasswordToken } from "../interfaces";
import { NotificationService } from "../services";
import { PlatformLocation } from "@angular/common";
import { Router } from "@angular/router";

@Component({
    templateUrl: "./tokenresetpassword.component.html",
    styleUrls: ["./login.component.scss"],
})
export class TokenResetPasswordComponent implements OnInit {

    public form: UntypedFormGroup;
    public customizationSettings: ICustomizationSettings;
    public baseUrl: string;

    constructor(private identityService: IdentityService, private router: Router, private route: ActivatedRoute, private notify: NotificationService,
                private fb: UntypedFormBuilder, private location: PlatformLocation, private customizationFacade: CustomizationFacade) {

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
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
        this.customizationFacade.settings$().subscribe(x => this.customizationSettings = x);
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notify.error("Email address is required");
            return;
        }
        const token = form.value as IResetPasswordToken;
        this.identityService.resetPassword(token).subscribe(x => {
            if (x.successful) {
                this.notify.success(`Your Password has been reset`);
                this.router.navigate(["login"]);
            } else {
                x.errors.forEach((val) => {
                    this.notify.error(val);
                });
            }
        });
    }
}
