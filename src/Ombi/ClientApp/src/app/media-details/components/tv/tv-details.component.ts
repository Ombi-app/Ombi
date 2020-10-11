import { Component, ViewEncapsulation, OnInit } from "@angular/core";
import { ImageService, SearchV2Service, MessageService, RequestService, SonarrService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { EpisodeRequestComponent } from "../../../shared/episode-request/episode-request.component";
import { IAdvancedData, IChildRequests, ISonarrProfile, ISonarrRootFolder, ITvRequests, RequestType } from "../../../interfaces";
import { AuthService } from "../../../auth/auth.service";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";

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
    public advancedOptions: IAdvancedData;
    public showAdvanced: boolean; // Set on the UI

    private tvdbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, public messageService: MessageService, private requestService: RequestService,
        private auth: AuthService, private sonarrService: SonarrService) {
        this.route.params.subscribe((params: any) => {
            this.tvdbId = params.tvdbId;
            this.fromSearch = params.search;
        });
    }

    public async ngOnInit() {
        await this.load();
    }

    public async load() {

        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        if (this.fromSearch) {
            this.tv = await this.searchService.getTvInfoWithMovieDbId(this.tvdbId);
            this.tvdbId = this.tv.id;
        } else {
            this.tv = await this.searchService.getTvInfo(this.tvdbId);
        }

        if (this.tv.requestId) {
            this.tvRequest = await this.requestService.getChildRequests(this.tv.requestId).toPromise();
            this.showRequest = this.tvRequest.length > 0 ? this.tvRequest[0].parentRequest : undefined;
        }

        const tvBanner = await this.imageService.getTvBanner(this.tvdbId).toPromise();
        this.tv.background = this.sanitizer.bypassSecurityTrustStyle("url(" + tvBanner + ")");
    }

    public async request() {
        this.dialog.open(EpisodeRequestComponent, { width: "800px", data: this.tv, panelClass: 'modal-panel' })
    }

    public async issue() {
        const dialogRef = this.dialog.open(NewIssueComponent, {
            width: '500px',
            data: { requestId: this.tvRequest ? this.tv.requestId : null, requestType: RequestType.tvShow, providerId: this.tv.theTvDbId, title: this.tv.title }
        });
    }

    public openDialog() {
        let trailerLink = this.tv.trailer;
        trailerLink = trailerLink.split('?v=')[1];

        this.dialog.open(YoutubeTrailerComponent, {
            width: '560px',
            data: trailerLink
        });
    }

    public setAdvancedOptions(data: IAdvancedData) {
        this.advancedOptions = data;
        console.log(this.advancedOptions);
        if (data.rootFolderId) {
            this.showRequest.qualityOverrideTitle = data.rootFolders.filter(x => x.id == data.rootFolderId)[0].path;
        }
        if (data.profileId) {
            this.showRequest.rootPathOverrideTitle = data.profiles.filter(x => x.id == data.profileId)[0].name;
        }
    }
}
