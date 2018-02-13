import { Component, Input, OnInit} from "@angular/core";
import "rxjs/add/operator/takeUntil";

import { NotificationService } from "../services";
import { RequestService } from "../services";
import { SearchService } from "../services";

import { INewSeasonRequests, IRequestEngineResult } from "../interfaces";
import { IEpisodesRequests } from "../interfaces";
import { ISearchTvResult } from "../interfaces";

@Component({
    selector: "seriesinformation",
    templateUrl: "./seriesinformation.component.html",
    styleUrls: ["./seriesinformation.component.scss"],
})
export class SeriesInformationComponent implements OnInit {

    public result: IRequestEngineResult;
    public series: ISearchTvResult;
    public requestedEpisodes: IEpisodesRequests[] = [];

    @Input() private seriesId: number;

    constructor(private searchService: SearchService, private requestService: RequestService, private notificationService: NotificationService) { }

    public ngOnInit() {
        this.searchService.getShowInformation(this.seriesId)
            .subscribe(x => {
                this.series = x;
            });
    }

    public submitRequests() {
        // Make sure something has been selected
        const selected = this.series.seasonRequests.some((season) => {
            return  season.episodes.some((ep) => {
                return ep.selected;
            });
        });

        if(!selected) {
            this.notificationService.error("You need to select some episodes!");
            return;
        }

        this.series.requested = true;

        this.requestService.requestTv(this.series)
            .subscribe(x => {
                this.result = x as IRequestEngineResult;
                if (this.result.result) {
                    this.notificationService.success(
                        `Request for ${this.series.title} has been added successfully`);

                    this.series.seasonRequests.forEach((season) => {
                        season.episodes.forEach((ep) => {
                            ep.selected = false;
                        });
                    });

                } else {
                    this.notificationService.warning("Request Added", this.result.errorMessage ? this.result.errorMessage : this.result.message);
                }
            });
    }

    public addRequest(episode: IEpisodesRequests) {
        episode.requested = true;
        episode.selected = true;
    }

    public removeRequest(episode: IEpisodesRequests) {
        episode.requested = false;
        episode.selected = false;
    }

    public addAllEpisodes(season: INewSeasonRequests) {
        season.episodes.forEach((ep) => this.addRequest(ep));
    }
}
