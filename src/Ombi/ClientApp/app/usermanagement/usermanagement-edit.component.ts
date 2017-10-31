import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { ConfirmationService } from "primeng/primeng";

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
                private notificationService: NotificationService,
                private router: Router,
                private confirmationService: ConfirmationService) {
        this.route.params
            .subscribe((params: any) => {
                this.userId = params.id;

                this.identityService.getUserById(this.userId).subscribe(x => {
                    this.user = x;
                });
            });
    }

    public delete() {

        this.confirmationService.confirm({
            message: "Are you sure that you want to delete this user? If this user has any requests they will also be deleted.",
            header: "Are you sure?",
            icon: "fa fa-trash",
            accept: () => {
                this.identityService.deleteUser(this.user).subscribe(x => {
                    if (x.successful) {
                        this.notificationService.success(`The user ${this.user.userName} was deleted`);
                        this.router.navigate(["usermanagement"]);
                    } else {
                        x.errors.forEach((val) => {
                            this.notificationService.error(val);
                        });
                    }
        
                });
            },
            reject: () => {
                return;
            },
        });        
    }

    public resetPassword() {
        this.identityService.submitResetPassword(this.user.emailAddress).subscribe(x => {
            if (x.successful) {
                this.notificationService.success(`Sent reset password email to ${this.user.emailAddress}`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error(val);
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
            this.notificationService.error("Please assign a role");
            return;
        }

        this.identityService.updateUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationService.success(`The user ${this.user.userName} has been updated successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error(val);
                });
            }
        });
    }

}
