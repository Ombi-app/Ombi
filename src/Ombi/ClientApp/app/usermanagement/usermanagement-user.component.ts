import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { ICheckbox, IUser, UserType } from "../interfaces";
import { IdentityService, NotificationService } from "../services";

import { ConfirmationService } from "primeng/primeng";

@Component({
    templateUrl: "./usermanagement-user.component.html",
})
export class UserManagementUserComponent implements OnInit {

    public user: IUser;
    public userId: string;
    public availableClaims: ICheckbox[];
    public confirmPass: "";
    
    public edit: boolean;

    constructor(private identityService: IdentityService,
                private notificationService: NotificationService,
                private router: Router,
                private route: ActivatedRoute,
                private confirmationService: ConfirmationService) {
                    this.route.params
            .subscribe((params: any) => {
                if(params.id) {
                    this.userId = params.id;
                    this.edit = true;
                    this.identityService.getUserById(this.userId).subscribe(x => {
                        this.user = x;
                    });
            }
            });
                    
                 }

    public ngOnInit() {
        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        if(!this.edit) {
            this.user = {
                alias: "",
                claims: [],
                emailAddress: "",
                id: "",
                password: "",
                userName: "",
                userType: UserType.LocalUser,
                checked: false,
                hasLoggedIn: false,
                lastLoggedIn: new Date(),
                episodeRequestLimit: 0,
                movieRequestLimit: 0,
                userAccessToken: "",
        };
    }
    }

    public create() {
        this.user.claims = this.availableClaims;

        if (this.user.password) {
            if (this.user.password !== this.confirmPass) {
                this.notificationService.error("Passwords do not match");
                return;
            }
        }
        const hasClaims = this.availableClaims.some((item) => {
            if (item.enabled) { return true; }

            return false;
        });

        if (!hasClaims) {
            this.notificationService.error("Please assign a role");
            return;
        }

        this.identityService.createUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationService.success(`The user ${this.user.userName} has been created successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error(val);
                });
            }
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
