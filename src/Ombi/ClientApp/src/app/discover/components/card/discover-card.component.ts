import { Component, OnInit, Input } from "@angular/core";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType, ISearchTvResult, ISearchMovieResult } from "../../../interfaces";
import { SearchV2Service } from "../../../services";
import { MatDialog } from "@angular/material/dialog";
import { DiscoverCardDetailsComponent } from "./discover-card-details.component";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";

@Component({
    selector: "discover-card",
    templateUrl: "./discover-card.component.html",
    styleUrls: ["./discover-card.component.scss"],
})
export class DiscoverCardComponent implements OnInit {

    @Input() public result: IDiscoverCardResult;
    public RequestType = RequestType;

    constructor(private searchService: SearchV2Service, private dialog: MatDialog) { }

    public ngOnInit() {
        if (this.result.type == RequestType.tvShow) {
            this.getExtraTvInfo();
        }
        if (this.result.type == RequestType.movie) {
            this.getExtraMovieInfo();
        }
    }

    public openDetails(details: IDiscoverCardResult) {
        this.dialog.open(DiscoverCardDetailsComponent, { width: "700px", data: details, panelClass: 'modal-panel' })
    }

    public async getExtraTvInfo() {
        var result = await this.searchService.getTvInfo(this.result.id);
        this.setTvDefaults(result);
        this.updateTvItem(result);

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
        return "notrequested";
    }

    private getExtraMovieInfo() {
        if (!this.result.imdbid) {
            this.searchService.getFullMovieDetails(this.result.id)
                .subscribe(m => {
                    this.updateMovieItem(m);
                });
        }
    }

    private updateMovieItem(updated: ISearchMovieResultV2) {
        this.result.url = "http://www.imdb.com/title/" + updated.imdbId + "/";
        this.result.available = updated.available;
        this.result.requested = updated.requested;
        this.result.requested = updated.requestProcessing;
        this.result.rating = updated.voteAverage;
    }


    private setTvDefaults(x: ISearchTvResultV2) {
        if (!x.imdbId) {
            x.imdbId = "https://www.tvmaze.com/shows/" + x.seriesId;
        } else {
            x.imdbId = "http://www.imdb.com/title/" + x.imdbId + "/";
        }
    }

    private updateTvItem(updated: ISearchTvResultV2) {
        this.result.title = updated.title;
        this.result.id = updated.id;
        this.result.available = updated.fullyAvailable;
        this.result.posterPath = updated.banner;
        this.result.requested = updated.requested;
        this.result.url = updated.imdbId;
    }

}
