import { Component, OnInit, Input } from "@angular/core";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType } from "../../../interfaces";
import { MessageService, RequestService, SearchV2Service } from "../../../services";
import { MatDialog } from "@angular/material/dialog";
import { DiscoverCardDetailsComponent } from "./discover-card-details.component";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";
import { EpisodeRequestComponent } from "../../../shared/episode-request/episode-request.component";

@Component({
    selector: "discover-card",
    templateUrl: "./discover-card.component.html",
    styleUrls: ["./discover-card.component.scss"],
})
export class DiscoverCardComponent implements OnInit {

    @Input() public result: IDiscoverCardResult;
    public RequestType = RequestType;
    public hide: boolean;
    public fullyLoaded = false;
    public loading: boolean;

    // This data is needed to open the dialog
    private tvSearchResult: ISearchTvResultV2;

    constructor(private searchService: SearchV2Service, private dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService) { }

    public ngOnInit() {
        if (this.result.type == RequestType.tvShow) {
            this.getExtraTvInfo();
        }
        if (this.result.type == RequestType.movie) {
            this.getExtraMovieInfo();
        }
        if (this.result.type == RequestType.album) {
            this.getAlbumInformation();
        }
    }

    public openDetails(details: IDiscoverCardResult) {
        this.dialog.open(DiscoverCardDetailsComponent, { width: "700px", data: details, panelClass: 'modal-panel' })
    }

    public async getExtraTvInfo() {
        if (this.result.tvMovieDb) {
            this.tvSearchResult = await this.searchService.getTvInfoWithMovieDbId(+this.result.id);
        } else {
            this.tvSearchResult = await this.searchService.getTvInfo(+this.result.id);
        }
        if (!this.tvSearchResult || this.tvSearchResult?.status.length > 0 && this.tvSearchResult?.status === "404") {
            this.hide = true;
            return;
        }

        this.setTvDefaults(this.tvSearchResult);
        this.updateTvItem(this.tvSearchResult);

    }

    public async getAlbumInformation() {
        this.searchService.getArtistInformation(this.result.id.toString()).subscribe(x => {
            if (x.poster) {
                this.result.posterPath = x.poster;
                this.fullyLoaded = true;
            } else {
                this.searchService.getReleaseGroupArt(this.result.id.toString()).subscribe(art => {
                    if (art.image) {
                        this.result.posterPath = art.image;

                        this.fullyLoaded = true;
                    }
                })
            }
            this.result.title = x.startYear ? `${x.name} (${x.startYear})` : x.name;
            this.result.overview = x.overview;
        });
    }

    public generateDetailsLink(): string {
        switch (this.result.type) {
            case RequestType.movie:
                return `/details/movie/${this.result.id}`;
            case RequestType.tvShow:
                return `/details/tv/${this.result.id}`;
            case RequestType.album: //Actually artist
                return `/details/artist/${this.result.id}`;
        }
    }

    public getStatusClass(): string {
        if (this.result.available) {
            return "available";
        }
        if (this.result.approved) {
            return "approved";
        }
        if (this.result.requested) {
            return "requested";
        }
        return "";
    }

    public getAvailbilityStatus(): string {
        if (this.result.available) {
            return "Available";
        }
        if (this.result.approved) {
            return "Approved";
        }
        if (this.result.requested) {
            return "Pending";
        }
        return "";
    }

    public request(event: any) {
        event.preventDefault();
        this.loading = true;
        switch (this.result.type) {
            case RequestType.tvShow:
                const dia = this.dialog.open(EpisodeRequestComponent, { width: "700px", data: { series: this.tvSearchResult }, panelClass: 'modal-panel' });
                dia.afterClosed().subscribe(x => this.loading = false);
                return;
            case RequestType.movie:
                this.requestService.requestMovie({ theMovieDbId: +this.result.id, languageCode: null, requestOnBehalf: null }).subscribe(x => {
                    if (x.result) {
                        this.result.requested = true;
                        this.messageService.send(x.message, "Ok");
                    } else {
                        this.messageService.send(x.errorMessage, "Ok");
                    }
                    this.loading = false;
                });
                return;
        }
    }

    private getExtraMovieInfo() {
        if (!this.result.imdbid) {
            this.searchService.getFullMovieDetails(+this.result.id)
                .subscribe(m => {
                    this.updateMovieItem(m);
                });
        } else {
            this.fullyLoaded = true;
        }
    }

    private updateMovieItem(updated: ISearchMovieResultV2) {
        this.result.url = "http://www.imdb.com/title/" + updated.imdbId + "/";
        this.result.available = updated.available;
        this.result.requested = updated.requested;
        this.result.requested = updated.requestProcessing;
        this.result.rating = updated.voteAverage;
        this.result.overview = updated.overview;
        this.result.imdbid = updated.imdbId;

        this.fullyLoaded = true;
    }


    private setTvDefaults(x: ISearchTvResultV2) {
        if (x.imdbId) {
            x.imdbId = "http://www.imdb.com/title/" + x.imdbId + "/";
        } else {
            x.imdbId = "https://www.tvmaze.com/shows/" + x.seriesId;
        }
    }

    private updateTvItem(updated: ISearchTvResultV2) {
        this.result.title = updated.title;
        this.result.id = updated.id;
        this.result.available = updated.fullyAvailable || updated.partlyAvailable;
        this.result.posterPath = updated.banner;
        this.result.requested = updated.requested;
        this.result.url = updated.imdbId;
        this.result.overview = updated.overview;
        this.result.approved = updated.approved;

        this.fullyLoaded = true;
    }

}
