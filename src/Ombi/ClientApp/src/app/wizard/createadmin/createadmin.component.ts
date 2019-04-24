import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";

import { PlatformLocation } from "@angular/common";
import { IdentityService } from "../../services";
import { NotificationService } from "../../services";

@Component({
    templateUrl: "./createadmin.component.html",
})
export class CreateAdminComponent implements OnInit {

    public username: string;
    public password: string;
    public baseUrl: string;

    constructor(private identityService: IdentityService, private notificationService: NotificationService,
                private router: Router, private location: PlatformLocation) { }

    public ngOnInit(): void {
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
    }

    public createUser() {
        this.identityService.createWizardUser({ username: this.username, password: this.password, usePlexAdminAccount: false }).subscribe(x => {
            if (x.result) {
                this.router.navigate(["login"]);
            } else {
                if (x.errors.length > 0) {
                    this.notificationService.error(x.errors[0]);
                }
            }
        });
    }
}
