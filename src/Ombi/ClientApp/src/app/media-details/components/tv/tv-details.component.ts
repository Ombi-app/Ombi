import { Component, ViewEncapsulation, OnInit } from "@angular/core";
import { ImageService, SearchV2Service, MessageService, RequestService, SonarrService, SettingsStateService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { EpisodeRequestComponent, EpisodeRequestData } from "../../../shared/episode-request/episode-request.component";
import { IAdvancedData, IChildRequests, ITvRequests, RequestType } from "../../../interfaces";
import { AuthService } from "../../../auth/auth.service";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { TvAdvancedOptionsComponent } from "./panels/tv-advanced-options/tv-advanced-options.component";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { RequestBehalfComponent } from "../shared/request-behalf/request-behalf.component";
import { forkJoin } from "rxjs";
import { TopBannerComponent } from "../shared/top-banner/top-banner.component";

@Component({
    templateUrl: "./tv-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class TvDetailsComponent implements OnInit {

    public tv: ISearchTvResultV2;
    public tvRequest: IChildRequests[];
    public showRequest: ITvRequests;
    public fromSearch: boolean;
    public isAdmin: boolean;
    public manageOwnRequests: boolean;
    public advancedOptions: IAdvancedData;
    public showAdvanced: boolean; // Set on the UI
    public requestType = RequestType.tvShow;
    public issuesEnabled: boolean;

    private tvdbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, public messageService: MessageService, private requestService: RequestService,
        private requestService2: RequestServiceV2,
        private auth: AuthService, private sonarrService: SonarrService, private settingsState: SettingsStateService) {
        this.route.params.subscribe((params: any) => {
            this.tvdbId = params.tvdbId;
            this.fromSearch = params.search;
        });
    }

    public async ngOnInit() {
        await this.load();
    }

    public async load() {

        this.issuesEnabled = this.settingsState.getIssue();
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.manageOwnRequests = this.auth.hasRole('ManageOwnRequests');

        if (this.isAdmin) {
            this.showAdvanced = await this.sonarrService.isEnabled();
        }

        // if (this.fromSearch) {
        //     this.tv = await this.searchService.getTvInfoWithMovieDbId(this.tvdbId);
        //     this.tvdbId = this.tv.id;
        // } else {
            this.tv = await this.searchService.getTvInfo(this.tvdbId);
        // }

        if (this.tv.requestId) {
            this.tvRequest = await this.requestService.getChildRequests(this.tv.requestId).toPromise();
            this.showRequest = this.tvRequest.length > 0 ? this.tvRequest[0].parentRequest : undefined;
            this.loadAdvancedInfo();
        }

        // const tvBanner = await this.imageService.getTvBanner(this.tvdbId).toPromise();
        this.tv.background = this.sanitizer.bypassSecurityTrustStyle("url(https://image.tmdb.org/t/p/original" + this.tv.banner + ")");
    }

    public async request(userId: string) {
        this.dialog.open(EpisodeRequestComponent, { width: "800px", data: <EpisodeRequestData> { series: this.tv, requestOnBehalf: userId, isAdmin: this.isAdmin }, panelClass: 'modal-panel' })
    }

    public async issue() {
        const dialogRef = this.dialog.open(NewIssueComponent, {
            width: '500px',
            data: { requestId: this.tvRequest ? this.tv.requestId : null, requestType: RequestType.tvShow, providerId: this.tv.theTvDbId, title: this.tv.title, posterPath: this.tv.images.original }
        });
    }

    public openDialog() {
        let trailerLink = this.tv.trailer;

        this.dialog.open(YoutubeTrailerComponent, {
            width: '560px',
            data: trailerLink
        });
    }

    public async openAdvancedOptions() {
        const dialog = this.dialog.open(TvAdvancedOptionsComponent, { width: "700px", data: <IAdvancedData>{ tvRequest: this.showRequest }, panelClass: 'modal-panel' })
        await dialog.afterClosed().subscribe(async result => {
            if (result) {
                // get the name and ids
                result.rootFolder = result.rootFolders.filter(f => f.id === +result.rootFolderId)[0];
                result.profile = result.profiles.filter(f => f.id === +result.profileId)[0];
                result.language = result.languages.filter(x => x.id === +result.langaugeId)[0];
                await this.requestService2.updateTvAdvancedOptions({ qualityOverride: result.profileId, rootPathOverride: result.rootFolderId, languageProfile: result.languageId, requestId: this.showRequest.id }).toPromise();
                this.setAdvancedOptions(result);
            }
        });
    }

    public setAdvancedOptions(data: IAdvancedData) {
        this.advancedOptions = data;
        console.log(this.advancedOptions);
        if (data.rootFolderId) {
            this.showRequest.qualityOverrideTitle = data.profiles.filter(x => x.id == data.profileId)[0].name;
        }
        if (data.profileId) {
            this.showRequest.rootPathOverrideTitle =  data.rootFolders.filter(x => x.id == data.rootFolderId)[0].path;
        }
        if (data.languageId) {
            this.showRequest.languageOverrideTitle =  data.languages.filter(x => x.id == data.languageId)[0].name;
        }
    }

    private loadAdvancedInfo() {
        const profile = this.sonarrService.getQualityProfilesWithoutSettings();
        const folders = this.sonarrService.getRootFoldersWithoutSettings();
        const languages = this.sonarrService.getV3LanguageProfilesWithoutSettings();

        forkJoin([profile, folders, languages]).subscribe(x => {
            const sonarrProfiles = x[0];
            const sonarrRootFolders = x[1];
            const languageProfiles = x[2];

            const profile = sonarrProfiles.filter((p) => {
                return p.id === this.showRequest.qualityOverride;
            });
            if (profile.length > 0) {
                this.showRequest.qualityOverrideTitle = profile[0].name;
            }

            const path = sonarrRootFolders.filter((folder) => {
                return folder.id === this.showRequest.rootFolder;
            });
            if (path.length > 0) {
                this.showRequest.rootPathOverrideTitle = path[0].path;
            }

            const lang = languageProfiles.filter((folder) => {
                return folder.id === this.showRequest.languageProfile;
            });
            if (lang.length > 0) {
                this.showRequest.languageOverrideTitle = lang[0].name;
            }

        });
    }
}
