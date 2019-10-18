import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { ICheckbox, INotificationAgent, INotificationPreferences, IRadarrProfile, IRadarrRootFolder, ISonarrProfile, ISonarrRootFolder, IUser, UserType } from "../interfaces";
import { IdentityService, NotificationService, RadarrService, SonarrService } from "../services";

import { ConfirmationService } from "primeng/primeng";

@Component({
    templateUrl: "./usermanagement-user.component.html",
    styles: [`

    ::ng-deep ngb-accordion > div.card >  div.card-header {
        padding:0px;
    }
    ::ng-deep ngb-accordion > div.card {
        color:white;
        padding-top: 0px;
    }
    `],
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

    constructor(private identityService: IdentityService,
                private notificationService: NotificationService,
                private router: Router,
                private route: ActivatedRoute,
                private confirmationService: ConfirmationService,
                private sonarrService: SonarrService,
                private radarrService: RadarrService) {

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
        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        if(this.edit) {
            this.identityService.getNotificationPreferencesForUser(this.userId).subscribe(x => this.notificationPreferences = x);
        } else {
            this.identityService.getNotificationPreferences().subscribe(x => this.notificationPreferences = x);
        }
        this.sonarrService.getQualityProfilesWithoutSettings().subscribe(x => this.sonarrQualities = x);
        this.sonarrService.getRootFoldersWithoutSettings().subscribe(x => this.sonarrRootFolders = x);
        this.radarrService.getQualityProfilesFromSettings().subscribe(x => this.radarrQualities = x);
        this.radarrService.getRootFoldersFromSettings().subscribe(x => this.radarrRootFolders = x);

        if(!this.edit) {
            this.user = {
                alias: "",
                claims: [],
                emailAddress: "",
                id: "",
                password: "",
                userName: "",
                userType: UserType.LocalUser,
                checked: false,
                hasLoggedIn: false,
                lastLoggedIn: new Date(),
                episodeRequestLimit: 0,
                movieRequestLimit: 0,
                userAccessToken: "",
                musicRequestLimit: 0,
                episodeRequestQuota: null,
                movieRequestQuota: null,
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
                this.notificationService.error("Passwords do not match");
                return;
            }
        }
        const hasClaims = this.availableClaims.some((item) => {
            if (item.enabled) { return true; }

            return false;
        });

        if (!hasClaims) {
            this.notificationService.error("Please assign a role");
            return;
        }

        this.identityService.createUser(this.user).subscribe(x => {
            if (x.successful) {
                this.notificationService.success(`The user ${this.user.userName} has been created successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error(val);
                });
            }
        });
    }

    public delete() {

        this.confirmationService.confirm({
            message: "Are you sure that you want to delete this user? If this user has any requests they will also be deleted.",
            header: "Are you sure?",
            icon: "fa fa-trash",
            accept: () => {
                this.identityService.deleteUser(this.user).subscribe(x => {
                    if (x.successful) {
                        this.notificationService.success(`The user ${this.user.userName} was deleted`);
                        this.router.navigate(["usermanagement"]);
                    } else {
                        x.errors.forEach((val) => {
                            this.notificationService.error(val);
                        });
                    }

                });
            },
            reject: () => {
                return;
            },
        });
    }

    public resetPassword() {
        this.identityService.submitResetPassword(this.user.emailAddress).subscribe(x => {
            if (x.successful) {
                this.notificationService.success(`Sent reset password email to ${this.user.emailAddress}`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error(val);
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
            this.notificationService.error("Please assign a role");
            return;
        }

        this.identityService.updateUser(this.user).subscribe(x => {
            if (x.successful) {
                this.identityService.updateNotificationPreferences(this.notificationPreferences).subscribe();
                this.notificationService.success(`The user ${this.user.userName} has been updated successfully`);
                this.router.navigate(["usermanagement"]);
            } else {
                x.errors.forEach((val) => {
                    this.notificationService.error(val);
                });
            }
        });
    }

}
