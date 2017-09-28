import { Component } from "@angular/core";
import { Router } from "@angular/router";

import { ActivatedRoute } from "@angular/router";
import { IUser } from "../interfaces";
import { IdentityService } from "../services";
import { NotificationService } from "../services";

@Component({
    templateUrl: "./usermanagement-edit.component.html",
})
export class UserManagementEditComponent {
    public user: IUser;
    public userId: string;

    constructor(private identityService: IdentityService,
                private route: ActivatedRoute,
                private notificationSerivce: NotificationService,
                private router: Router) {
        this.route.params
            .subscribe((params: any) => {
                this.userId = params.id;

                this.identityService.getUserById(this.userId).subscribe(x => {
                    this.user = x;
                });
            });
    }

    public delete() {
        this.identityService.deleteUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationSerivce.success("Deleted", `The user ${this.user.username} was deleted`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationSerivce.error("Error", val);
                });
            }

        });
    }

    public resetPassword() {
        this.identityService.submitResetPassword(this.user.emailAddress).subscribe(x => {
            if (x.successful) {
                this.notificationSerivce.success("Reset", `Sent reset password email to ${this.user.emailAddress}`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationSerivce.error("Error", val);
                });
            }

        });
    }

    public update() {
        const hasClaims = this.user.claims.some((item) => {
            if (item.enabled) { return true; }

            return false;
        });

        if (!hasClaims) {
            this.notificationSerivce.error("Error", "Please assign a role");
            return;
        }

        this.identityService.updateUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationSerivce.success("Updated", `The user ${this.user.username} has been updated successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationSerivce.error("Error", val);
                });
            }
        });
    }

}
