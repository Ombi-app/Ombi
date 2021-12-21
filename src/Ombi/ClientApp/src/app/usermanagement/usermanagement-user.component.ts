﻿import { ActivatedRoute, Router } from "@angular/router";
import { Component, OnInit } from "@angular/core";
import { ICheckbox, ICustomizationSettings, INotificationAgent, INotificationPreferences, IRadarrProfile, IRadarrRootFolder, ISonarrProfile, ISonarrRootFolder, IUser, RequestLimitType, UserType } from "../interfaces";
import { IdentityService, MessageService, RadarrService, SettingsService, SonarrService } from "../services";

import { Clipboard } from '@angular/cdk/clipboard';
import { CustomizationFacade } from "../state/customization";
import { Location } from "@angular/common";

@Component({
    templateUrl: "./usermanagement-user.component.html",
    styleUrls: ["./usermanagement-user.component.scss"],
})
export class UserManagementUserComponent implements OnInit {

    public user: IUser;
    public userId: string;
    public availableClaims: ICheckbox[];
    public confirmPass: "";
    public notificationPreferences: INotificationPreferences[];

    public sonarrQualities: ISonarrProfile[];
    public sonarrRootFolders: ISonarrRootFolder[];
    public radarrQualities: IRadarrProfile[];
    public radarrRootFolders: IRadarrRootFolder[];

    public NotificationAgent = INotificationAgent;
    public edit: boolean;

    public countries: string[];
    public requestLimitTypes: RequestLimitType[];
    public RequestLimitType = RequestLimitType;

    private appUrl: string = this.customizationFacade.appUrl();
    private accessToken: string;

    constructor(private identityService: IdentityService,
                private notificationService: MessageService,
                private readonly settingsService: SettingsService,
                private router: Router,
                private route: ActivatedRoute,
                private sonarrService: SonarrService,
                private radarrService: RadarrService,
                private clipboard: Clipboard,
                private location: Location,
                private customizationFacade: CustomizationFacade,
                ) {

                    this.route.params.subscribe((params: any) => {
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
        this.requestLimitTypes = [RequestLimitType.Day, RequestLimitType.Week, RequestLimitType.Month];
        this.identityService.getSupportedStreamingCountries().subscribe(x => this.countries = x);
        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        if(this.edit) {
            this.identityService.getNotificationPreferencesForUser(this.userId).subscribe(x => this.notificationPreferences = x);
        } else {
            this.identityService.getNotificationPreferences().subscribe(x => this.notificationPreferences = x);
        }
        this.sonarrService.getQualityProfilesWithoutSettings().subscribe(x => {
            this.sonarrQualities = x;
            this.sonarrQualities.unshift({id: 0, name: "None"});
        });
        this.sonarrService.getRootFoldersWithoutSettings().subscribe(x => {
            this.sonarrRootFolders = x;
            this.sonarrRootFolders.unshift({id: 0, path: "None"});
        });
        this.radarrService.getQualityProfilesFromSettings().subscribe(x => {
            this.radarrQualities = x;
            this.radarrQualities.unshift({id: 0, name: "None"});
        });
        this.radarrService.getRootFoldersFromSettings().subscribe(x => {
            this.radarrRootFolders = x;
            this.radarrRootFolders.unshift({id: 0, path: "None"});
        });

        this.identityService.getUserAccessToken(this.userId).subscribe(x => this.accessToken = x);

        if(!this.edit) {
            this.user = {
                alias: "",
                claims: [],
                emailAddress: "",
                id: "",
                password: "",
                userName: "",
                userType: UserType.LocalUser,
                hasLoggedIn: false,
                lastLoggedIn: new Date(),
                episodeRequestLimit: 0,
                movieRequestLimit: 0,
                userAccessToken: "",
                musicRequestLimit: 0,
                episodeRequestQuota: null,
                movieRequestQuota: null,
                language: null,
                userAlias: "",
                streamingCountry: "US",
                userQualityProfiles: {
                    radarrQualityProfile: 0,
                    radarrRootPath: 0,
                    sonarrQualityProfile: 0,
                    sonarrQualityProfileAnime: 0,
                    sonarrRootPath: 0,
                    sonarrRootPathAnime: 0,
                },
                musicRequestQuota: null,
        };
    }
    }

    public create() {
        this.user.claims = this.availableClaims;

        if (this.user.password) {
            if (this.user.password !== this.confirmPass) {
                this.notificationService.send("Passwords do not match");
                return;
            }
        }
        const hasClaims = this.availableClaims.some((item) => {
            if (item.enabled) { return true; }

            return false;
        });

        if (!hasClaims) {
            this.notificationService.send("Please assign a role");
            return;
        }

        this.identityService.createUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationService.send(`The user ${this.user.userName} has been created successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.send(val);
                });
            }
        });
    }

    public delete() {

        this.identityService.deleteUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationService.send(`The user ${this.user.userName} was deleted`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.send(val);
                });
            }

        });
    }

    public resetPassword() {
        this.identityService.submitResetPassword(this.user.emailAddress).subscribe(x => {
            if (x.successful) {
                this.notificationService.send(`Sent reset password email to ${this.user.emailAddress}`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.send(val);
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
            this.notificationService.send("Please assign a role");
            return;
        }


        this.identityService.updateUser(this.user).subscribe(x => {
            if (x.successful) {
                this.identityService.updateNotificationPreferences(this.notificationPreferences).subscribe();
                this.notificationService.send(`The user ${this.user.userName} has been updated successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.send(val);
                });
            }
        });
    }

    public async appLink() {
        this.clipboard.copy(`ombi://${this.appUrl}|${this.accessToken}`);
        this.notificationService.send("Copied!");
    }

    public back() {
        this.location.back();
    }

}
