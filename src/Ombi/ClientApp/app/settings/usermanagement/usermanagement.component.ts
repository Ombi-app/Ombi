import { Component, OnInit } from "@angular/core";

import { ICheckbox, IUserManagementSettings } from "../../interfaces";
import { IdentityService, SettingsService } from "../../services";

@Component({
    templateUrl: "./usermanagement.component.html",
})
export class UserManagementComponent implements OnInit {

    public plexEnabled: boolean;
    public embyEnabled: boolean;
    public settings: IUserManagementSettings;
    public claims: ICheckbox[];

    constructor(private settingsService: SettingsService,
                //private notificationService: NotificationService,
                private identityService: IdentityService) {

    }

    public ngOnInit(): void {
        this.settingsService.getUserManagementSettings().subscribe(x => {
            this.settings = x;
            this.identityService.getAllAvailableClaims().subscribe(c => {

                this.claims = c;
                this.claims.forEach((claim) => {
                    if (this.settings.defaultClaims) {
                        const hasClaim = this.settings.defaultClaims.some((item) => {
                            return item.value === claim.value && item.enabled;
                        });
                        claim.enabled = hasClaim;
                    }
                });
            });
        });
        this.settingsService.getPlex().subscribe(x => this.plexEnabled = x.enable);
        this.settingsService.getEmby().subscribe(x => this.embyEnabled = x.enable);

    }
}
