import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./authentication.component.html",
    styleUrls: ["./authentication.component.scss"],
})
export class AuthenticationComponent implements OnInit {

    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private fb: FormBuilder) { }

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
            });
        });
    }

    public onSubmit(form: FormGroup) {
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
