import { Component, OnInit } from "@angular/core";

import { IMassEmailUserModel } from "../../interfaces";
import { IdentityService } from "../../services";

@Component({
    templateUrl: "./massemail.component.html",
})
export class MassEmailComponent implements OnInit {

    public users: IMassEmailUserModel[] = [];
    public message: string;

    constructor(
                private readonly identityService: IdentityService) {
    }

    public ngOnInit(): void {
       this.identityService.getUsers().subscribe(x => {
        x.forEach(u => {
            this.users.push({
                user: u,
                selected: false,
            });
        });
       });
    }

    public selectAllUsers() {
        this.users.forEach(u => u.selected = !u.selected);
    }

    public selectSingleUser(user: IMassEmailUserModel) {
        user.selected = !user.selected;
    }

}
