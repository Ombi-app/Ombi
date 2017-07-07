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
    private router : Router) { }

    user: IUser;
    availableClaims: ICheckbox[];

    ngOnInit(): void {
        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        this.user = {
            alias: "",
            claims: [],
            emailAddress: "",
            id: "",
            password: "",
            username: "",
            userType: UserType.LocalUser
        }
    }

    update(): void {
        this.user.claims = this.availableClaims;
        this.identityService.createUser(this.user).subscribe(x => {
            this.notificationSerivce.success("Updated", `The user ${this.user.username} has been created successfully`)
            this.router.navigate(['usermanagement']);
        })
    }

}