import { Component, OnInit } from "@angular/core";

import { ICheckbox, IUserManagementSettings } from "../../interfaces";
import { IPlexFriends } from "../../interfaces/IPlex";
import { IdentityService, JobService, NotificationService, PlexService, SettingsService } from "../../services";

@Component({
    templateUrl: "./usermanagement.component.html",
})
export class UserManagementComponent implements OnInit {

    public plexEnabled: boolean;
    public embyEnabled: boolean;
    public settings: IUserManagementSettings;
    public claims: ICheckbox[];

    public plexUsers: IPlexFriends[];
    public filteredPlexUsers: IPlexFriends[];
    public bannedPlexUsers: IPlexFriends[] = [];

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private identityService: IdentityService,
                private plexService: PlexService,
                private jobService: JobService) {

    }

    public ngOnInit(): void {
        this.settingsService.getUserManagementSettings().subscribe(x => {
            this.settings = x;

            this.plexService.getFriends().subscribe(f => { 
                this.plexUsers = f;
                this.plexUsers.forEach((plex) => {
                    const isExcluded = this.settings.bannedPlexUserIds.some((val) => {
                        return plex.id === val;
                    });
                    if (isExcluded) {
                    this.bannedPlexUsers.push(plex);
                }
                });
            });

            this.identityService.getAllAvailableClaims().subscribe(c => {

                this.claims = c;
                this.claims.forEach((claim) => {
                    if (this.settings.defaultRoles) {
                        const hasClaim = this.settings.defaultRoles.some((item) => {
                            return item === claim.value;
                        });
                        claim.enabled = hasClaim;
                    }
                });
            });
        });
        this.settingsService.getPlex().subscribe(x => this.plexEnabled = x.enable);
        this.settingsService.getEmby().subscribe(x => this.embyEnabled = x.enable);
    }

    public submit(): void {
        const enabledClaims = this.claims.filter((claim) => {
            return claim.enabled;
        });
        this.settings.defaultRoles = enabledClaims.map((claim) => claim.value);
        this.settings.bannedPlexUserIds = this.bannedPlexUsers.map((u) => u.id);

        this.settingsService.saveUserManagementSettings(this.settings).subscribe(x => {
            if (x === true) {
                this.notificationService.success("Saved", "Successfully saved the User Management Settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Ombi settings");
            }
        });
    }

    public filterUserList(event: any) {
        this.filteredPlexUsers = this.filter(event.query, this.plexUsers);
    }

    public runImporter(): void {
        this.jobService.runPlexImporter().subscribe();
    }

    private filter(query: string, users: IPlexFriends[]): IPlexFriends[] {
        return users.filter((val) => {
            return val.username.toLowerCase().indexOf(query.toLowerCase()) === 0;
        });
    }
}
