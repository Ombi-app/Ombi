import { Component, OnInit, Input } from "@angular/core";
import { Router } from "@angular/router";

import { PlatformLocation } from "@angular/common";
import { IdentityService } from "../../services";
import { NotificationService } from "../../services";
import { ICreateWizardUser } from "../../interfaces";

@Component({
    selector: "wizard-local-admin",
    templateUrl: "./createadmin.component.html",
})
export class CreateAdminComponent implements OnInit {

    @Input() user: ICreateWizardUser;

    public username: string;
    public password: string;
    public baseUrl: string;
    

    constructor(private location: PlatformLocation) { }

    public ngOnInit(): void {
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
    }

}
