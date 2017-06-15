import { Component, OnInit } from '@angular/core';

import { IUser, ICheckbox } from '../interfaces/IUser';
import { IdentityService } from '../services/identity.service';

@Component({
  
    templateUrl: './usermanagement.component.html'
})
export class UserManagementComponent implements OnInit {
    constructor(private identityService: IdentityService) { }

    ngOnInit(): void {
        this.users = [];
        this.identityService.getUsers().subscribe(x => {
            this.users = x;
        });
        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);

        this.resetCreatedUser();
    }

    users: IUser[];
    selectedUser: IUser;
    createdUser: IUser;

    availableClaims : ICheckbox[];

    showEditDialog = false;
    showCreateDialogue = false;
    
    edit(user: IUser) {
        this.selectedUser = user;
        this.showEditDialog = true;
    }

    updateUser() {
        this.showEditDialog = false;
        this.identityService.updateUser(this.selectedUser).subscribe(x => this.selectedUser = x);
    }

    create() {
        this.createdUser.claims = this.availableClaims;
        this.identityService.createUser(this.createdUser).subscribe(x => {
            this.users.push(x); // Add the new user

            this.showCreateDialogue = false;
            this.resetCreatedUser();
        });

    }

    private resetClaims() {
        //this.availableClaims.forEach(x => {
        //    x.enabled = false;
        //});
    }

    private resetCreatedUser() {
        this.createdUser = {
            id: "-1",
            alias: "",
            claims: [],
            emailAddress: "",
            password: "",
            userType: 1,
            username: "",

        }
        this.resetClaims();
    }

    //private removeRequestFromUi(key : IRequestModel) {
    //    var index = this.requests.indexOf(key, 0);
    //    if (index > -1) {
    //        this.requests.splice(index, 1);
    //    }
    //}
}