import { Component, Inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from "@angular/material/dialog";
import { IAdvancedData, IRadarrProfile, IRadarrRootFolder, RequestCombination } from "../../../../../interfaces";
import { RadarrService } from "../../../../../services";
import { MatButtonModule } from "@angular/material/button";
import { MatOptionModule } from "@angular/material/core";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { TranslateModule } from "@ngx-translate/core";

@Component({
        standalone: true,
    templateUrl: "./movie-advanced-options.component.html",
    selector: "movie-advanced-options",
    imports: [
        CommonModule,
        TranslateModule,
        MatFormFieldModule,
        MatSelectModule,
        MatOptionModule,
        MatDialogModule,
        MatButtonModule
    ]
})
export class MovieAdvancedOptionsComponent implements OnInit {

    public radarrProfiles: IRadarrProfile[];
    public radarrProfiles4K: IRadarrProfile[];
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
        if (this.showNormal) {
            this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
                this.radarrProfiles = c;
                this.data.profiles = c;
                this.data.profileId ??= this.data.movieRequest.qualityOverride;
                this.setQualityOverrideTitle(c, this.data.movieRequest.qualityOverride);
            });
        }
        if (this.show4k) {
            this.radarrService.getQualityProfiles4kFromSettings().subscribe(c => {
                if (this.showNormal) {
                    this.radarrProfiles4K = c;
                    this.data.profiles4K = c;
                    this.data.profileId4K ??= this.data.movieRequest.qualityOverride4K;
                } else {
                    this.radarrProfiles = c;
                    this.data.profiles = c;
                    this.data.profileId ??= this.data.movieRequest.qualityOverride4K;
                    this.setQualityOverrideTitle(c, this.data.movieRequest.qualityOverride4K);
                }
            });
            this.radarrService.getRootFolders4kFromSettings().subscribe(c => {
                this.radarrRootFolders = c;
                this.data.rootFolders = c;
                this.data.rootFolderId ??= this.data.movieRequest.rootPathOverride;
                this.setRootFolderOverrides();
            });
        } else {
            this.radarrService.getRootFoldersFromSettings().subscribe(c => {
                this.radarrRootFolders = c;
                this.data.rootFolders = c;
                this.data.rootFolderId ??= this.data.movieRequest.rootPathOverride;
                this.setRootFolderOverrides();
            });
        }
    }

    private setQualityOverrideTitle(profiles: IRadarrProfile[], qualityOverride: number): void {
        const profile = profiles.find(p => p.id === qualityOverride);
        if (profile) {
            this.data.movieRequest.qualityOverrideTitle = profile.name;
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
