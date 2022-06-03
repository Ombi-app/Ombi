import { Component, OnInit, ViewChild } from "@angular/core";
import { IdentityService, NotificationService, SettingsService } from "../../services";

import { CustomizationFacade } from "../../state/customization/customization.facade";
import { ICreateWizardUser } from "../../interfaces";
import { IOmbiConfigModel } from "../models/OmbiConfigModel";
import { MatStepper } from'@angular/material/stepper';
import { Router } from "@angular/router";
import { WizardService } from "../services/wizard.service";

@Component({
    templateUrl: "./welcome.component.html",
    styleUrls: ["./welcome.component.scss"],
})
export class WelcomeComponent implements OnInit {

    @ViewChild('stepper', {static: false}) public stepper: MatStepper;
    public localUser: ICreateWizardUser;
    public config: IOmbiConfigModel;

    constructor(private router: Router, private identityService: IdentityService,
        private notificationService: NotificationService, private WizardService: WizardService,
        private settingsService: SettingsService, private customizationFacade: CustomizationFacade) { }

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
        if (this.config.applicationUrl) {
            this.settingsService.verifyUrl(this.config.applicationUrl).subscribe(x => {
                    if (!x) {
                        this.notificationService.error(`The URL "${this.config.applicationUrl}" is not valid. Please format it correctly e.g. http://www.google.com/`);
                        this.stepper.selectedIndex = 3;
                        return;
                    }
                    this.saveConfig();
                });
            } else {
                this.saveConfig();
            }
    }

    private saveConfig() {
        this.WizardService.addOmbiConfig(this.config).subscribe({
            next: (config) => {
                    if(config != null) {
                    this.identityService.createWizardUser(this.localUser).subscribe(x => {
                    if (x.result) {
                        this.customizationFacade.loadCustomziationSettings().subscribe();
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
    },
    error: (configErr) => this.notificationService.error(configErr)
    });
    }

}
