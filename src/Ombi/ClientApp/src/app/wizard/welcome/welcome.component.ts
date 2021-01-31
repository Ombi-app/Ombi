import { AfterViewInit, Component, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { ICreateWizardUser } from "../../interfaces";
import { IdentityService, NotificationService } from "../../services";
import { IOmbiConfigModel } from "../models/OmbiConfigModel";
import { WizardService } from "../services/wizard.service";
import { MatHorizontalStepper } from'@angular/material/stepper';
import { StepperSelectionEvent } from "@angular/cdk/stepper";

@Component({
    templateUrl: "./welcome.component.html",
    styleUrls: ["./welcome.component.scss"],
})
export class WelcomeComponent implements OnInit {

    @ViewChild('stepper', {static: false}) public stepper: MatHorizontalStepper;
    public localUser: ICreateWizardUser;
    public config: IOmbiConfigModel;

    constructor(private router: Router, private identityService: IdentityService,
        private notificationService: NotificationService, private WizardService: WizardService) { }

    public ngOnInit(): void {
        this.localUser = {
            password:"",
            username:"",
            usePlexAdminAccount:false
        }
        this.config = {
            applicationName: null,
            applicationUrl: null,
            logo: null
        };
    }

    public createUser() {
        this.WizardService.addOmbiConfig(this.config).subscribe(config => {
            if(config != null) {
                this.identityService.createWizardUser(this.localUser).subscribe(x => {
                if (x.result) {
                // save the config
                this.router.navigate(["login"]);
            } else {
                if (x.errors.length > 0) {
                    this.notificationService.error(x.errors[0]);
                    this.stepper.previous();
                }
            }
        });
    }
    }, configErr => this.notificationService.error(configErr));
    }
}
