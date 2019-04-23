import { Component, Input, OnInit } from "@angular/core";
import { RadarrService } from "../../../../../services";
import { IRadarrProfile, IRadarrRootFolder, IMovieRequests, IAdvancedData } from "../../../../../interfaces";
import { MatDialog } from "@angular/material";
import { MovieAdvancedOptionsComponent } from "../movie-advanced-options/movie-advanced-options.component";

@Component({
    templateUrl: "./movie-admin-panel.component.html",
    selector: "movie-admin-panel",
})
export class MovieAdminPanelComponent implements OnInit {

    @Input() public movie: IMovieRequests;

    public radarrProfiles: IRadarrProfile[];
    public selectedRadarrProfile: IRadarrProfile;
    public radarrRootFolders: IRadarrRootFolder[];
    public selectRadarrRootFolders: IRadarrRootFolder;

    constructor(private radarrService: RadarrService, private dialog: MatDialog) { }

    public async ngOnInit() {
        if (await this.radarrService.isRadarrEnabled()) {
            this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
                this.radarrProfiles = c;
                this.setQualityOverrides();
            });
            this.radarrService.getRootFoldersFromSettings().subscribe(c => {
                this.radarrRootFolders = c;
                this.setRootFolderOverrides();
            });
        }
    }

    public openAdvancedOptions() {
        const dialog = this.dialog.open(MovieAdvancedOptionsComponent, { width: "700px", data: <IAdvancedData>{ profiles: this.radarrProfiles, rootFolders: this.radarrRootFolders }, panelClass: 'modal-panel' })
        dialog.afterClosed().subscribe(result => {
            console.log(result);
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
