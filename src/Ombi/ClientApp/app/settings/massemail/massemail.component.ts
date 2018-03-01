import { Component, OnInit } from "@angular/core";

import { IMassEmailModel, IUser } from "../../interfaces";
import { IdentityService, NotificationService, SettingsService } from "../../services";

@Component({
    templateUrl: "./massemail.component.html",
})
export class MassEmailComponent implements OnInit {

    public users: IMassEmailModel[];

    constructor(private readonly settingsService: SettingsService,
                private readonly notificationService: NotificationService,
                private readonly identityService: IdentityService) {
    }

    public ngOnInit(): void {
       this.identityService.getUsers().subscribe(x => {
        x.forEach(u => {
            this.users.push({
                user: u,
                selected: true,
            });
        });
       });
    }

    public selectAllUsers() {
        this.users.forEach(u => u.selected = true);
    }

    public selectSingleUser(user: IMassEmailModel) {
        user.selected = true;
    }

}
