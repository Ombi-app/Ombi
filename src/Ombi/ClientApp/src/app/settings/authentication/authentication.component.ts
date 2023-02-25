import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./authentication.component.html",
    styleUrls: ["./authentication.component.scss"],
})
export class AuthenticationComponent implements OnInit {

    public form: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: UntypedFormBuilder) { }

    public ngOnInit() {
        this.settingsService.getAuthentication().subscribe(x => {
            this.form = this.fb.group({
                allowNoPassword: [x.allowNoPassword],
                requiredDigit: [x.requiredDigit],
                requiredLength: [x.requiredLength],
                requiredLowercase: [x.requiredLowercase],
                requireNonAlphanumeric: [x.requireNonAlphanumeric],
                requireUppercase: [x.requireUppercase],
                enableOAuth: [x.enableOAuth],
                enableHeaderAuth: [x.enableHeaderAuth],
                headerAuthVariable: [x.headerAuthVariable],
                headerAuthCreateUser: [x.headerAuthCreateUser],
            });
            this.form.controls.enableHeaderAuth.valueChanges.subscribe(x => {
                if (x) {
                    this.form.get("headerAuthVariable").setValidators(Validators.required);
                } else {
                    this.form.get("headerAuthVariable").clearValidators();
                }
                this.form.get("headerAuthVariable").updateValueAndValidity();
            });
        });


    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        this.settingsService.saveAuthentication(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Authentication settings");
            } else {
                this.notificationService.success("There was an error when saving the Authentication settings");
            }
        });
    }
}
