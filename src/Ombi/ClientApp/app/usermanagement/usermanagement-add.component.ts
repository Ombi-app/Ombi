import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { IUser, UserType, ICheckbox } from '../interfaces/IUser';
import { IdentityService } from '../services/identity.service';
import { NotificationService } from '../services/notification.service';

@Component({

    templateUrl: './usermanagement-add.component.html'
})
export class UserManagementAddComponent implements OnInit {
    constructor(private identityService: IdentityService,
        private notificationSerivce: NotificationService,
        private router: Router) { }

    user: IUser;
    availableClaims: ICheckbox[];
    confirmPass: "";

    ngOnInit(): void {
        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        this.user = {
            alias: "",
            claims: [],
            emailAddress: "",
            id: "",
            password: "",
            username: "",
            userType: UserType.LocalUser,
            checked:false,
            isSetup:false
        }
    }

    create(): void {
        this.user.claims = this.availableClaims;

        if (this.user.password) {
            if (this.user.password !== this.confirmPass) {
                this.notificationSerivce.error("Error", "Passwords do not match");
                return;
            }
        }
        var hasClaims = this.availableClaims.some((item) => {
            if (item.enabled) { return true; }

            return false;
        });

        if (!hasClaims) {
            this.notificationSerivce.error("Error", "Please assign a role");
            return;
        }

        this.identityService.createUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationSerivce.success("Updated", `The user ${this.user.username} has been created successfully`)
                this.router.navigate(['usermanagement']);
            } else {
                x.errors.forEach((val) => {
                    this.notificationSerivce.error("Error", val);
                });
            }
        })
    }

}