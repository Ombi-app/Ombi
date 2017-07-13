import { Component } from '@angular/core';
import { Router } from '@angular/router';

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
        private notificationSerivce: NotificationService,
        private router: Router) {
        this.route.params
            .subscribe(params => {
                this.userId = params['id'];

                this.identityService.getUserById(this.userId).subscribe(x => {
                    this.user = x;
                });
            });
    }

    user: IUser;
    userId: string;

    delete(): void {
        this.identityService.deleteUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationSerivce.success("Deleted", `The user ${this.user.username} was deleted`)
                this.router.navigate(['usermanagement']);
            } else {
                x.errors.forEach((val) => {
                    this.notificationSerivce.error("Error", val);
                });
            }

        });
    }

    update(): void {
        var hasClaims = this.user.claims.some((item) => {
            if (item.enabled) { return true; }

            return false;
        });

        if (!hasClaims) {
            this.notificationSerivce.error("Error", "Please assign a role");
            return;
        }

        this.identityService.updateUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationSerivce.success("Updated", `The user ${this.user.username} has been updated successfully`)
                this.router.navigate(['usermanagement']);
            } else {
                x.errors.forEach((val) => {
                    this.notificationSerivce.error("Error", val);
                });
            }
        })
    }

}