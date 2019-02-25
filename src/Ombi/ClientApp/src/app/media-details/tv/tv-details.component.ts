import { Component, ViewEncapsulation } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchTvResultV2 } from "../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material";
import { YoutubeTrailerComponent } from "../youtube-trailer.component";

@Component({
    templateUrl: "./tv-details.component.html",
    styleUrls: ["../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class TvDetailsComponent {
    public tv: ISearchTvResultV2;
    private tvdbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService) {
        this.route.params.subscribe((params: any) => {
            this.tvdbId = params.tvdbId;
            this.load();
        });
    }

    public async load() {
        this.tv = await this.searchService.getTvInfo(this.tvdbId);
        const tvBanner = await this.imageService.getTvBanner(this.tvdbId).toPromise();
        this.tv.background = this.sanitizer.bypassSecurityTrustStyle("url(" + tvBanner + ")");
    }

    public async request() {
        // var result = await this.requestService.requestTv({}).toPromise();
        // if (result.result) {
        //     this.movie.requested = true;
        //     this.messageService.send(result.message, "Ok");
        // } else {
        //     this.messageService.send(result.errorMessage, "Ok");
        // }
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
