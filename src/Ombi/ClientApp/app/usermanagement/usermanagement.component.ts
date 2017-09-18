import { Component, OnInit } from "@angular/core";

import { IUser } from "../interfaces";
import { IdentityService } from "../services";

@Component({
    templateUrl: "./usermanagement.component.html",
})
export class UserManagementComponent implements OnInit {

    public users: IUser[];
    public checkAll = false;

    constructor(private identityService: IdentityService) { }

    public ngOnInit() {
        this.users = [];
        this.identityService.getUsers().subscribe(x => {
            this.users = x;
        });

    }

    public welcomeEmail(user: IUser) {
        // todo
    }

    public checkAllBoxes() {
        this.checkAll = !this.checkAll;
        this.users.forEach(user => {
            user.checked = this.checkAll;
        });
    }
}
