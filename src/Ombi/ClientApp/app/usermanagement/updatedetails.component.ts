import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';

import { IUpdateLocalUser } from '../interfaces/IUser';
import { IdentityService } from '../services/identity.service';
import { NotificationService } from '../services/notification.service';

@Component({
    templateUrl: './updatedetails.component.html'
})
export class UpdateDetailsComponent implements OnInit {
    constructor(private identityService: IdentityService,
        private notificationService: NotificationService,
     private fb: FormBuilder) { }

    form: FormGroup;

    ngOnInit(): void {
        this.identityService.getUser().subscribe(x => {
            var localUser = x as IUpdateLocalUser;
             this.form = this.fb.group({
                 id:[localUser.id],
                username: [localUser.username],
                emailAddress: [localUser.emailAddress, [Validators.email]],
                confirmNewPassword: [localUser.confirmNewPassword],
                currentPassword: [localUser.currentPassword, [Validators.required]],
                password: [localUser.password],
            });

            
        });

    }

    onSubmit(form : FormGroup) {
    if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return
        }

        if (form.controls["password"].dirty) {
            if (form.value.password !== form.value.confirmNewPassword) {
                this.notificationService.error("Error", "Passwords do not match");
                return;
            }
        }

        this.identityService.updateLocalUser(this.form.value).subscribe(x => {
            if (x.successful) {
                this.notificationService.success("Updated", `All of your details have now been updated`)
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error("Error", val);
                });
            }
        });

    }

     
}