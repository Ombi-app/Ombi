import { Component, Input, OnInit, EventEmitter, Output } from "@angular/core";
import { RadarrService } from "../../../../../services";
import { IRadarrProfile, IRadarrRootFolder, IMovieRequests, IAdvancedData } from "../../../../../interfaces";
import { MatDialog } from "@angular/material/dialog";
import { MovieAdvancedOptionsComponent } from "../movie-advanced-options/movie-advanced-options.component";
import { RequestServiceV2 } from "../../../../../services/requestV2.service";

@Component({
    templateUrl: "./movie-admin-panel.component.html",
    selector: "movie-admin-panel",
})
export class MovieAdminPanelComponent implements OnInit {

    @Input() public movie: IMovieRequests;
    @Output() public advancedOptionsChanged = new EventEmitter<IAdvancedData>();
    @Output() public radarrEnabledChange = new EventEmitter<boolean>();

    public radarrEnabled: boolean;
    public radarrProfiles: IRadarrProfile[];
    public selectedRadarrProfile: IRadarrProfile;
    public radarrRootFolders: IRadarrRootFolder[];
    public selectRadarrRootFolders: IRadarrRootFolder;

    constructor(private radarrService: RadarrService, private requestService: RequestServiceV2, private dialog: MatDialog) { }

    public async ngOnInit() {
        this.radarrEnabled = await this.radarrService.isRadarrEnabled();
        if (this.radarrEnabled) {
            this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
                this.radarrProfiles = c;
                this.setQualityOverrides();
            });
            this.radarrService.getRootFoldersFromSettings().subscribe(c => {
                this.radarrRootFolders = c;
                this.setRootFolderOverrides();
            });
        }

        this.radarrEnabledChange.emit(this.radarrEnabled);
    }

    public async openAdvancedOptions() {
        const dialog = this.dialog.open(MovieAdvancedOptionsComponent, { width: "700px", data: <IAdvancedData>{ profiles: this.radarrProfiles, rootFolders: this.radarrRootFolders }, panelClass: 'modal-panel' })
        await dialog.afterClosed().subscribe(async result => {
            if(result) {
                // get the name and ids
                result.rootFolder = result.rootFolders.filter(f => f.id === +result.rootFolderId)[0];
                result.profile = result.profiles.filter(f => f.id === +result.profileId)[0];
                await this.requestService.updateMovieAdvancedOptions({qualityOverride: result.profileId, rootPathOverride: result.rootFolderId, requestId: this.movie.id}).toPromise();
                this.advancedOptionsChanged.emit(result);
            }
        });
    }

    private setQualityOverrides(): void {
        if (this.radarrProfiles) {
            const profile = this.radarrProfiles.filter((p) => {
                return p.id === this.movie.qualityOverride;
            });
            if (profile.length > 0) {
                this.movie.qualityOverrideTitle = profile[0].name;
            }
        }
    }

    private setRootFolderOverrides(): void {
        if (this.radarrRootFolders) {
            const path = this.radarrRootFolders.filter((folder) => {
                return folder.id === this.movie.rootPathOverride;
            });
            if (path.length > 0) {
                this.movie.rootPathOverrideTitle = path[0].path;
            }
        }
    }
}
