import { Component, Input, OnInit } from "@angular/core";
import { MessageService, RequestService, SearchV2Service } from "../../../services";

import { AdminRequestDialogComponent } from "../../../shared/admin-request-dialog/admin-request-dialog.component";
import { DiscoverType } from "../carousel-list/carousel-list.component";
import { EpisodeRequestComponent } from "../../../shared/episode-request/episode-request.component";
import { IDiscoverCardResult } from "../../interfaces";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material/dialog";
import { RequestType } from "../../../interfaces";
import { TranslateService } from "@ngx-translate/core";

@Component({
    selector: "discover-card",
    templateUrl: "./discover-card.component.html",
    styleUrls: ["./discover-card.component.scss"],
})
export class DiscoverCardComponent implements OnInit {

    @Input() public discoverType: DiscoverType;
    @Input() public result: IDiscoverCardResult;
    @Input() public isAdmin: boolean;
    @Input() public is4kEnabled: boolean = false;
    public RequestType = RequestType;
    public hide: boolean;
    public fullyLoaded = false;
    public loading: boolean;
    public allow4KButton: boolean = false;

    public requestable: boolean;

    // This data is needed to open the dialog
    private tvSearchResult: ISearchTvResultV2;

    constructor(private searchService: SearchV2Service, private dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService, private translate: TranslateService) { }

    public ngOnInit() {
        if (this.result.type == RequestType.tvShow) {
            this.fullyLoaded = true;
            this.getExtraTvInfo();
        }
        if (this.result.type == RequestType.movie) {
            this.allow4KButton = true;
            this.getExtraMovieInfo();
        }
        if (this.result.type == RequestType.album) {
            this.getAlbumInformation();
        }
    }

    public async getExtraTvInfo() {
        this.tvSearchResult = await this.searchService.getTvInfoWithMovieDbId(+this.result.id);
        this.requestable = true;
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

                    }
                })
            }
            this.result.title = x.startYear ? `${x.name} (${x.startYear})` : x.name;
            this.result.overview = x.overview;
            this.fullyLoaded = true;
            this.requestable = true;
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
        if (this.tvSearchResult?.partlyAvailable) {
            return "partly-available";
        }
        if (this.result.approved) {
            return "approved";
        }
        if (this.result.denied) {
            return "denied";
        }
        if (this.result.requested) {
            return "requested";
        }
        return "";
    }

    public getAvailabilityStatus(): string {
        if (this.result.available) {
            return this.translate.instant("Common.Available");
        }
        if (this.tvSearchResult?.partlyAvailable) {
            return this.translate.instant("Common.PartlyAvailable");
        }
        if (this.result.approved) {
            return this.translate.instant("Common.Approved");
        }
        if (this.result.denied) {
            return this.translate.instant("Common.Denied");
        }
        if (this.result.requested) {
            return this.translate.instant("Common.Pending");
        }
        return "";
    }

    public request(event: any, is4k: boolean) {
        event.preventDefault();
        this.loading = true;
        switch (this.result.type) {
            case RequestType.tvShow:
                const dia = this.dialog.open(EpisodeRequestComponent, { width: "700px", data: { series: this.tvSearchResult, isAdmin: this.isAdmin }, panelClass: 'modal-panel' });
                dia.afterClosed().subscribe(x => this.loading = false);
                return;
            case RequestType.movie:
                if (this.isAdmin) {
                    const dialog = this.dialog.open(AdminRequestDialogComponent, { width: "700px", data: { type: RequestType.movie, id: this.result.id,  }, panelClass: 'modal-panel' });
                    dialog.afterClosed().subscribe((result) => {
                        if (result) {
                                this.requestService.requestMovie({ theMovieDbId: +this.result.id,
                                    languageCode: this.translate.currentLang,
                                    qualityPathOverride: result.radarrPathId,
                                    requestOnBehalf: result.username?.id,
                                    rootFolderOverride: result.radarrFolderId,
                                    is4KRequest: is4k }).subscribe(x => {
                                if (x.result) {
                                    this.result.requested = true;
                                    this.messageService.send(this.translate.instant("Requests.RequestAddedSuccessfully", { title: this.result.title }), "Ok");
                                } else {
                                    this.messageService.sendRequestEngineResultError(x);
                                }
                            });
                        }
                    });
                } else {
                this.requestService.requestMovie({ theMovieDbId: +this.result.id, languageCode: this.translate.currentLang, requestOnBehalf: null, qualityPathOverride: null, rootFolderOverride: null, is4KRequest: is4k }).subscribe(x => {
                    if (x.result) {
                        this.result.requested = true;
                        this.messageService.send(this.translate.instant("Requests.RequestAddedSuccessfully", { title: this.result.title }), "Ok");
                    } else {
                        this.messageService.sendRequestEngineResultError(x);
                    }
                    this.loading = false;
                });
                return;
            }
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
        this.requestable = true;
    }

    private updateMovieItem(updated: ISearchMovieResultV2) {
        this.result.url = "http://www.imdb.com/title/" + updated.imdbId + "/";
        this.result.available = updated.available || updated.available4K;
        this.result.requested = updated.requested || updated.has4KRequest;
        this.result.approved = updated.approved || updated.available4K;
        this.result.denied = updated.denied || updated.denied4K;
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
        // this.result.available = updated.fullyAvailable || updated.partlyAvailable;
        // this.result.posterPath = updated.banner;
        this.result.requested = updated.requested;
        this.result.url = updated.imdbId;
        this.result.overview = updated.overview;
        this.result.approved = updated.approved;
        this.result.available = updated.fullyAvailable;

        this.fullyLoaded = true;
    }

}
