import { Component, Input } from "@angular/core";
import { ICreateWizardUser } from "../../interfaces";

@Component({
    selector: "wizard-local-admin",
    templateUrl: "./createadmin.component.html",
    styleUrls: ["../welcome/welcome.component.scss"]
})
export class CreateAdminComponent {

    @Input() user: ICreateWizardUser;

    public username: string;
    public password: string;    

    constructor() { }
}
