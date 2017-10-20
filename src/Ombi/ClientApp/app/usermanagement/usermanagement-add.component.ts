import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";

import { ICheckbox, IUser, UserType } from "../interfaces";
import { IdentityService, NotificationService } from "../services";

@Component({
    templateUrl: "./usermanagement-add.component.html",
})
export class UserManagementAddComponent implements OnInit {
    public user: IUser;
    public availableClaims: ICheckbox[];
    public confirmPass: "";

    constructor(private identityService: IdentityService,
                private notificationSerivce: NotificationService,
                private router: Router) { }

    public ngOnInit() {
        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        this.user = {
            alias: "",
            claims: [],
            emailAddress: "",
            id: "",
            password: "",
            userName: "",
            userType: UserType.LocalUser,
            checked:false,
            hasLoggedIn: false,
            lastLoggedIn:new Date(),
        };
    }

    public create() {
        this.user.claims = this.availableClaims;

        if (this.user.password) {
            if (this.user.password !== this.confirmPass) {
                this.notificationSerivce.error("Error", "Passwords do not match");
                return;
            }
        }
        const hasClaims = this.availableClaims.some((item) => {
            if (item.enabled) { return true; }

            return false;
        });

        if (!hasClaims) {
            this.notificationSerivce.error("Error", "Please assign a role");
            return;
        }

        this.identityService.createUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationSerivce.success("Updated", `The user ${this.user.userName} has been created successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationSerivce.error("Error", val);
                });
            }
        });
    }

}
