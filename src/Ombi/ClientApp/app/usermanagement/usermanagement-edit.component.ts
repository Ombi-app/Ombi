import { Component } from '@angular/core';

import { IUser } from '../interfaces/IUser';
import { IdentityService } from '../services/identity.service';
import { NotificationService } from '../services/notification.service';
import { ActivatedRoute } from '@angular/router';

@Component({

    templateUrl: './usermanagement-edit.component.html'
})
export class UserManagementEditComponent {
    constructor(private identityService: IdentityService,
        private route: ActivatedRoute,
        private notificationSerivce: NotificationService) {
        this.route.params
            .subscribe(params => {
                this.userId = +params['id']; // (+) converts string 'id' to a number

                this.identityService.getUserById(this.userId).subscribe(x => {
                    this.user = x;
                });
            });
    }

    user: IUser;
    userId: number;

    update(): void {
        this.identityService.updateUser(this.user).subscribe(x => {

            this.notificationSerivce.success("Updated",`The user ${this.user.username} has been updated successfully`)
        })
    }
 
}