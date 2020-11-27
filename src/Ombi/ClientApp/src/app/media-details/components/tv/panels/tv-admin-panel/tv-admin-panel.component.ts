import { Component, Input, OnInit, EventEmitter, Output } from "@angular/core";
import { RadarrService, SonarrService } from "../../../../../services";
import { IRadarrProfile, IRadarrRootFolder, IAdvancedData, ITvRequests, ISonarrProfile, ISonarrRootFolder } from "../../../../../interfaces";
import { MatDialog } from "@angular/material/dialog";

import { RequestServiceV2 } from "../../../../../services/requestV2.service";
import { MovieAdvancedOptionsComponent } from "../../../movie/panels/movie-advanced-options/movie-advanced-options.component";

@Component({
    templateUrl: "./tv-admin-panel.component.html",
    selector: "tv-admin-panel",
})
export class TvAdminPanelComponent implements OnInit {

    @Input() public tv: ITvRequests;
    @Output() public advancedOptionsChanged = new EventEmitter<IAdvancedData>();
    @Output() public sonarrEnabledChange = new EventEmitter<boolean>();

    public sonarrEnabled: boolean;
    public radarrProfiles: IRadarrProfile[];
    public selectedRadarrProfile: IRadarrProfile;
    public radarrRootFolders: IRadarrRootFolder[];
    public selectRadarrRootFolders: IRadarrRootFolder;


    public sonarrProfiles: ISonarrProfile[];
    public sonarrRootFolders: ISonarrRootFolder[];

    constructor(private sonarrService: SonarrService, private requestService: RequestServiceV2, private dialog: MatDialog) { }

    public async ngOnInit() {
        this.sonarrEnabled = await this.sonarrService.isEnabled();
        if (this.sonarrEnabled) {
            this.sonarrService.getQualityProfilesWithoutSettings()
                .subscribe(x => {
                    this.sonarrProfiles = x;
                    this.setQualityOverrides();
                });
            this.sonarrService.getRootFoldersWithoutSettings()
                .subscribe(x => {
                    this.sonarrRootFolders = x;
                    this.setRootFolderOverrides();
                });
        }

        this.sonarrEnabledChange.emit(this.sonarrEnabled);
    }

    public async openAdvancedOptions() {
        const dialog = this.dialog.open(MovieAdvancedOptionsComponent, { width: "700px", data: <IAdvancedData>{ profiles: this.sonarrProfiles, rootFolders: this.sonarrRootFolders }, panelClass: 'modal-panel' })
        await dialog.afterClosed().subscribe(async result => {
            if (result) {
                // get the name and ids
                result.rootFolder = result.rootFolders.filter(f => f.id === +result.rootFolderId)[0];
                result.profile = result.profiles.filter(f => f.id === +result.profileId)[0];
                await this.requestService.updateTvAdvancedOptions({ qualityOverride: result.profileId, rootPathOverride: result.rootFolderId, requestId: this.tv.id }).toPromise();
                this.advancedOptionsChanged.emit(result);
            }
        });
    }

    private setQualityOverrides(): void {
        if (this.sonarrProfiles) {
            const profile = this.sonarrProfiles.filter((p) => {
                return p.id === this.tv.qualityOverride;
            });
            if (profile.length > 0) {
                this.tv.qualityOverrideTitle = profile[0].name;
            }
        }
    }

    private setRootFolderOverrides(): void {
        if (this.sonarrRootFolders) {
            const path = this.sonarrRootFolders.filter((folder) => {
                return folder.id === this.tv.rootFolder;
            });
            if (path.length > 0) {
                this.tv.rootPathOverrideTitle = path[0].path;
            }
        }
    }
}
