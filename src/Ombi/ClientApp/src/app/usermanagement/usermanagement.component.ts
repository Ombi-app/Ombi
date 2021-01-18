import { AfterViewInit, Component, OnInit, ViewChild } from "@angular/core";

import { ICheckbox, ICustomizationSettings, IEmailNotificationSettings, IUser } from "../interfaces";
import { IdentityService, NotificationService, SettingsService } from "../services";
import { MatSort } from "@angular/material/sort";
import { MatTableDataSource } from "@angular/material/table";
import { SelectionModel } from "@angular/cdk/collections";

@Component({
    templateUrl: "./usermanagement.component.html",
    styleUrls: ["./usermanagement.component.scss"],
})
export class UserManagementComponent implements OnInit {

    public displayedColumns: string[] = ['select', 'username', 'alias', 'email', 'roles', 'remainingRequests',
        'nextRequestDue', 'lastLoggedIn', 'userType', 'actions', 'welcome'];
    public dataSource: MatTableDataSource<IUser>;

    public selection = new SelectionModel<IUser>(true, []);
    @ViewChild(MatSort) public sort: MatSort;
    public users: IUser[];
    public checkAll = false;
    public emailSettings: IEmailNotificationSettings;
    public customizationSettings: ICustomizationSettings;
    public showBulkEdit = false;
    public availableClaims: ICheckbox[];
    public bulkMovieLimit?: number;
    public bulkEpisodeLimit?: number;
    public bulkMusicLimit?: number;
    public bulkStreaming?: string;
    public plexEnabled: boolean;

    public countries: string[];

    constructor(private identityService: IdentityService,
        private settingsService: SettingsService,
        private notificationService: NotificationService,
        private plexSettings: SettingsService) {
            this.dataSource = new MatTableDataSource();
         }


    public async ngOnInit() {
        this.identityService.getSupportedStreamingCountries().subscribe(x => this.countries = x);
        this.users = await this.identityService.getUsers().toPromise();
        this.dataSource = new MatTableDataSource(this.users);
        this.dataSource.sort = this.sort;

        this.plexSettings.getPlex().subscribe(x => this.plexEnabled = x.enable);

        this.identityService.getAllAvailableClaims().subscribe(x => this.availableClaims = x);
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.settingsService.getEmailNotificationSettings().subscribe(x => this.emailSettings = x);
    }

    public welcomeEmail(user: IUser) {
        if (!user.emailAddress) {
            this.notificationService.error("The user needs an email address.");
            return;
        }
        if (!this.emailSettings.enabled) {
            this.notificationService.error("Email Notifications are not setup, cannot send welcome email");
            return;
        }
        if (!this.emailSettings.notificationTemplates.some(x => {
            return x.enabled && x.notificationType === 8;
        })) {
            this.notificationService.error("The Welcome Email template is not enabled in the Email Setings");
            return;
        }
        this.identityService.sendWelcomeEmail(user).subscribe();
        this.notificationService.success(`Sent a welcome email to ${user.emailAddress}`);
    }

    public bulkUpdate() {
        const anyRoles = this.availableClaims.some(x => {
            return x.enabled;
        });

        this.selection.selected.forEach(x => {
            if (anyRoles) {
                x.claims = this.availableClaims;
            }
            if (this.bulkEpisodeLimit) {
                x.episodeRequestLimit = this.bulkEpisodeLimit;
            }
            if (this.bulkMovieLimit) {
                x.movieRequestLimit = this.bulkMovieLimit;
            }
            if (this.bulkMusicLimit) {
                x.musicRequestLimit = this.bulkMusicLimit;
            }
            this.identityService.updateUser(x).subscribe(y => {
                if (!y.successful) {
                    this.notificationService.error(`Could not update user ${x.userName}. Reason ${y.errors[0]}`);
                }
            });
        });

        this.notificationService.success(`Updated users`);
        this.showBulkEdit = false;
        this.bulkMovieLimit = undefined;
        this.bulkEpisodeLimit = undefined;
        this.bulkMusicLimit = undefined;
    }

    public isAllSelected() {
        if (!this.dataSource) {
            return;
        }
        const numSelected = this.selection.selected.length;
        const numRows = this.dataSource.data.length;
        return numSelected === numRows;
    }


    public masterToggle() {
        if (!this.dataSource) {
            return;
        }
        this.isAllSelected() ?
            this.selection.clear() :
            this.dataSource.data.forEach(row => this.selection.select(row));
    }

    /** The label for the checkbox on the passed row */
    public checkboxLabel(row?: IUser): string {
        if (!row) {
            return `${this.isAllSelected() ? 'select' : 'deselect'} all`;
        }
        return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.id + 1}`;
    }
}
