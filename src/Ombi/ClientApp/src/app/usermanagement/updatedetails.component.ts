import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { IUpdateLocalUser } from "../interfaces";
import { IdentityService } from "../services";
import { NotificationService } from "../services";

@Component({
    templateUrl: "./updatedetails.component.html",
})
export class UpdateDetailsComponent implements OnInit {
    public form: FormGroup;

    constructor(private identityService: IdentityService,
                private notificationService: NotificationService,
                private fb: FormBuilder) { }

    public ngOnInit() {
        this.identityService.getUser().subscribe(x => {
            const localUser = x as IUpdateLocalUser;
            this.form = this.fb.group({
                 id: [localUser.id],
                username: [localUser.userName],
                emailAddress: [localUser.emailAddress, [Validators.email]],
                confirmNewPassword: [localUser.confirmNewPassword],
                currentPassword: [localUser.currentPassword, [Validators.required]],
                password: [localUser.password],
            });

        });

    }

    public onSubmit(form: FormGroup) {
    if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

    if (form.controls.password.dirty) {
            if (form.value.password !== form.value.confirmNewPassword) {
                this.notificationService.error("Passwords do not match");
                return;
            }
        }

    this.identityService.updateLocalUser(this.form.value).subscribe(x => {
            if (x.successful) {
                this.notificationService.success(`All of your details have now been updated`);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error(val);
                });
            }
        });

    }

}
