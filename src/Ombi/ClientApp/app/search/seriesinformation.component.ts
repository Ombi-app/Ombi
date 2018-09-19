import { Component, Input, OnInit } from "@angular/core";

import { NotificationService } from "../services";
import { RequestService } from "../services";
import { SearchService } from "../services";

import { INewSeasonRequests, IRequestEngineResult, ISeasonsViewModel, ITvRequestViewModel } from "../interfaces";
import { IEpisodesRequests } from "../interfaces";
import { ISearchTvResult } from "../interfaces";

import { Subject } from "rxjs";

@Component({
    selector: "seriesinformation",
    templateUrl: "./seriesinformation.component.html",
    styleUrls: ["./seriesinformation.component.scss"],
})
export class SeriesInformationComponent implements OnInit {

    public result: IRequestEngineResult;
    public series: ISearchTvResult;
    public requestedEpisodes: IEpisodesRequests[] = [];
    @Input() public tvRequested: Subject<void>;
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

        if (!selected) {
            this.notificationService.error("You need to select some episodes!");
            return;
        }

        this.series.requested = true;

        const viewModel = <ITvRequestViewModel> { firstSeason: this.series.firstSeason, latestSeason: this.series.latestSeason, requestAll: this.series.requestAll, tvDbId: this.series.id};
        viewModel.seasons = [];
        this.series.seasonRequests.forEach((season) => {
            const seasonsViewModel = <ISeasonsViewModel> {seasonNumber: season.seasonNumber, episodes: []};
            season.episodes.forEach(ep => {
                if (!this.series.latestSeason || !this.series.requestAll || !this.series.firstSeason) {
                    if (ep.selected) {
                        seasonsViewModel.episodes.push({episodeNumber: ep.episodeNumber});
                    }
                }
            });

            viewModel.seasons.push(seasonsViewModel);
        });

        this.requestService.requestTv(viewModel)
            .subscribe(x => {
                this.tvRequested.next();
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
