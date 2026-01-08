import { Component, OnInit, ViewChild } from "@angular/core";
import { IdentityService, NotificationService, SettingsService, StatusService } from "../../services";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatStepperModule } from "@angular/material/stepper";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatCardModule } from "@angular/material/card";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { CustomizationFacade } from "../../state/customization/customization.facade";
import { ICreateWizardUser } from "../../interfaces";
import { IOmbiConfigModel } from "../models/OmbiConfigModel";
import { MatStepper } from'@angular/material/stepper';
import { Router } from "@angular/router";
import { WizardService } from "../services/wizard.service";
import { Observable, take } from "rxjs";
import { MediaServerComponent } from "../mediaserver/mediaserver.component";
import { PlexComponent } from "../plex/plex.component";
import { EmbyComponent } from "../emby/emby.component";
import { JellyfinComponent } from "../jellyfin/jellyfin.component";
import { CreateAdminComponent } from "../createadmin/createadmin.component";
import { OmbiConfigComponent } from "../ombiconfig/ombiconfig.component";
import { DatabaseComponent } from "../database/database.component";
import { MatTabsModule } from "@angular/material/tabs";

@Component({
    standalone: true,
    templateUrl: "./welcome.component.html",
    styleUrls: ["./welcome.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatStepperModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatCardModule,
        MatIconModule,
        MatTooltipModule,
        TranslateModule,
        MatTabsModule,
        PlexComponent,
        EmbyComponent,
        JellyfinComponent,
        CreateAdminComponent,
        OmbiConfigComponent,
        DatabaseComponent
    ]
})
export class WelcomeComponent implements OnInit {

    @ViewChild('stepper', {static: false}) public stepper: MatStepper;
    public localUser: ICreateWizardUser;
    public needsRestart: boolean = false;
    public config: IOmbiConfigModel;

    constructor(private router: Router, private identityService: IdentityService,
        private notificationService: NotificationService, private WizardService: WizardService,
        private settingsService: SettingsService, private customizationFacade: CustomizationFacade,
        private status: StatusService) { }

    public ngOnInit(): void {
        this.status.getWizardStatus().pipe(take(1))
        .subscribe(x => {
            if (x.result) {
                this.router.navigate(["login"]);
            }
        });
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
                        this.stepper.selectedIndex = 4;
                        return;
                    }
                    this.saveConfig();
                });
            } else {
                this.saveConfig();
            }
    }

    public databaseConfigured() {
        this.needsRestart = true;
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
