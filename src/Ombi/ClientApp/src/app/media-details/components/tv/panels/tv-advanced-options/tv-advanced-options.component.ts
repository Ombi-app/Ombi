import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { IAdvancedData, ISonarrProfile, ISonarrRootFolder } from "../../../../../interfaces";
import { SonarrService } from "../../../../../services";

@Component({
    templateUrl: "./tv-advanced-options.component.html",
    selector: "tv-advanced-options",
})
export class TvAdvancedOptionsComponent implements OnInit {

    public sonarrProfiles: ISonarrProfile[];
    public sonarrRootFolders: ISonarrRootFolder[];

    constructor(public dialogRef: MatDialogRef<TvAdvancedOptionsComponent>, @Inject(MAT_DIALOG_DATA) public data: IAdvancedData,
        private sonarrService: SonarrService
    ) {
    }


    public async ngOnInit() {
        this.sonarrService.getQualityProfilesWithoutSettings().subscribe(c => {
            this.sonarrProfiles = c;
            this.data.profiles = c;
            this.setQualityOverrides();
        });
        this.sonarrService.getRootFoldersWithoutSettings().subscribe(c => {
            this.sonarrRootFolders = c;
            this.data.rootFolders = c;
            this.setRootFolderOverrides();
        });
    }

    private setQualityOverrides(): void {
        if (this.sonarrProfiles) {
            const profile = this.sonarrProfiles.filter((p) => {
                return p.id === this.data.tvRequest.qualityOverride;
            });
            if (profile.length > 0) {
                this.data.movieRequest.qualityOverrideTitle = profile[0].name;
            }
        }
    }

    private setRootFolderOverrides(): void {
        if (this.sonarrRootFolders) {
            const path = this.sonarrRootFolders.filter((folder) => {
                return folder.id === this.data.tvRequest.rootFolder;
            });
            if (path.length > 0) {
                this.data.movieRequest.rootPathOverrideTitle = path[0].path;
            }
        }
    }
}
