import { Component, ViewEncapsulation } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { EpisodeRequestComponent } from "../../../shared/episode-request/episode-request.component";

@Component({
    templateUrl: "./tv-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class TvDetailsComponent {
    public tv: ISearchTvResultV2;
    public fromSearch: boolean;

    public seasonCount: number;
    public totalEpisodes: number = 0;
    public nextEpisode: any;

    private tvdbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, public messageService: MessageService) {
        this.route.params.subscribe((params: any) => {
            this.tvdbId = params.tvdbId;
            this.fromSearch = params.search;

            this.load();
        });
    }

    public async load() {
        if(this.fromSearch) {
            this.tv = await this.searchService.getTvInfoWithMovieDbId(this.tvdbId);
            this.tvdbId = this.tv.id;
        } else {
            this.tv = await this.searchService.getTvInfo(this.tvdbId);
        }

        this.tv.seasonRequests.forEach(season => {
            this.totalEpisodes = this.totalEpisodes + season.episodes.length;
        });
        this.seasonCount = this.tv.seasonRequests.length;

        const tvBanner = await this.imageService.getTvBanner(this.tvdbId).toPromise();
        this.tv.background = this.sanitizer.bypassSecurityTrustStyle("url(" + tvBanner + ")");
    }

    public async request() {
        this.dialog.open(EpisodeRequestComponent, { width: "700px", data: this.tv })
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
