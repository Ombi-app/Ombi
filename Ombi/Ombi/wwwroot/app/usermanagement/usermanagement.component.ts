import { Component, OnInit } from '@angular/core';

import { IUser } from '../interfaces/IUser';
import { IdentityService } from '../services/identity.service';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './usermanagement.component.html'
})
export class UserManagementComponent implements OnInit {
    constructor(private identityService: IdentityService) { }

    ngOnInit(): void {
        this.users = [];
        this.identityService.getUsers().subscribe(x => {
            this.users = x;
        });
    }

    users: IUser[];
    selectedUser: IUser;


    edit(user: IUser) {
        this.selectedUser = user;
    }
    changeSort(username: string) {
        //??????
    }

    //private removeRequestFromUi(key : IRequestModel) {
    //    var index = this.requests.indexOf(key, 0);
    //    if (index > -1) {
    //        this.requests.splice(index, 1);
    //    }
    //}
}