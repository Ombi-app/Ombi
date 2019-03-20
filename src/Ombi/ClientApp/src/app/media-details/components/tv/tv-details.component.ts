import { Component, ViewEncapsulation, OnInit } from "@angular/core";
import { ImageService, SearchV2Service, MessageService, RequestService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { EpisodeRequestComponent } from "../../../shared/episode-request/episode-request.component";
import { IChildRequests } from "../../../interfaces";
import { AuthService } from "../../../auth/auth.service";

@Component({
    templateUrl: "./tv-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class TvDetailsComponent implements OnInit {

    public tv: ISearchTvResultV2;
    public tvRequest: IChildRequests[];
    public fromSearch: boolean;
    public isAdmin: boolean;

    private tvdbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, public messageService: MessageService, private requestService: RequestService,
        private auth: AuthService) {
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
        }

        const tvBanner = await this.imageService.getTvBanner(this.tvdbId).toPromise();
        this.tv.background = this.sanitizer.bypassSecurityTrustStyle("url(" + tvBanner + ")");
    }

    public async request() {
        this.dialog.open(EpisodeRequestComponent, { width: "800px", data: this.tv, panelClass: 'modal-panel' })
    }

    public openDialog() {
        debugger;
        let trailerLink = this.tv.trailer;
        trailerLink = trailerLink.split('?v=')[1];

        this.dialog.open(YoutubeTrailerComponent, {
            width: '560px',
            data: trailerLink
        });
    }
}
