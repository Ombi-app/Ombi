import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { IAdvancedData, IRadarrProfile, IRadarrRootFolder, RequestCombination } from "../../../../../interfaces";
import { RadarrService } from "../../../../../services";

@Component({
    templateUrl: "./movie-advanced-options.component.html",
    selector: "movie-advanced-options",
})
export class MovieAdvancedOptionsComponent implements OnInit {

    public radarrProfiles: IRadarrProfile[];
    public radarrRootFolders: IRadarrRootFolder[];
    public show4k: boolean = false;
    public showNormal: boolean = false;

    constructor(public dialogRef: MatDialogRef<MovieAdvancedOptionsComponent>, @Inject(MAT_DIALOG_DATA) public data: IAdvancedData,
        private radarrService: RadarrService
    ) {
    }


    public async ngOnInit() {
        this.show4k = this.data.movieRequest.requestCombination === RequestCombination.FourK || this.data.movieRequest.requestCombination === RequestCombination.Both;
        this.showNormal = this.data.movieRequest.requestCombination === RequestCombination.Normal || this.data.movieRequest.requestCombination === RequestCombination.Both;
        if (this.show4k) {
            this.radarrService.getQualityProfiles4kFromSettings().subscribe(c => {
                this.radarrProfiles = c;
                this.data.profiles4k = c;
                this.setQualityOverrides();
            });
            this.radarrService.getRootFolders4kFromSettings().subscribe(c => {
                this.radarrRootFolders = c;
                this.data.rootFolders4k = c;
                this.setRootFolderOverrides();
            });
        } else { // Currently show either 4k or normal, if it's a dual request there needs to be more work done to save the overrides for 4k separately
            this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
                this.radarrProfiles = c;
                this.data.profiles = c;
                this.setQualityOverrides();
            });
            this.radarrService.getRootFoldersFromSettings().subscribe(c => {
                this.radarrRootFolders = c;
                this.data.rootFolders = c;
                this.setRootFolderOverrides();
            });
        }
    }

    private setQualityOverrides(): void {
        if (this.radarrProfiles) {
            const profile = this.radarrProfiles.filter((p) => {
                return p.id === this.data.movieRequest.qualityOverride;
            });
            if (profile.length > 0) {
                this.data.movieRequest.qualityOverrideTitle = profile[0].name;
            }
        }
    }

    private setRootFolderOverrides(): void {
        if (this.radarrRootFolders) {
            const path = this.radarrRootFolders.filter((folder) => {
                return folder.id === this.data.movieRequest.rootPathOverride;
            });
            if (path.length > 0) {
                this.data.movieRequest.rootPathOverrideTitle = path[0].path;
            }
        }
    }
}
