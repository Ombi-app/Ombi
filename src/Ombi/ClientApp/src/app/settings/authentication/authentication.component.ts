import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { NotificationService } from "../../services";
import { SettingsMenuComponent } from "../settingsmenu.component";
import { WikiComponent } from "../wiki.component";
import { SettingsService } from "../../services";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule
    ],
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
