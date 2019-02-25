import { Component, OnInit, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatCheckboxChange } from "@angular/material";
import { ISearchTvResultV2 } from "../../interfaces/ISearchTvResultV2";
import { RequestService, NotificationService } from "../../services";
import { ITvRequestViewModel, ISeasonsViewModel, IEpisodesRequests, INewSeasonRequests } from "../../interfaces";


@Component({
    selector: "episode-request",
    templateUrl: "episode-request.component.html",
})
export class EpisodeRequestComponent implements OnInit {

    public loading: boolean;

    constructor(public dialogRef: MatDialogRef<EpisodeRequestComponent>, @Inject(MAT_DIALOG_DATA) public series: ISearchTvResultV2,
        private requestService: RequestService, private notificationService: NotificationService) { }

    public ngOnInit() {
        this.loading = true;

        this.loading = false;
    }

    public submitRequests() {
        // Make sure something has been selected
        const selected = this.series.seasonRequests.some((season) => {
            return season.episodes.some((ep) => {
                return ep.selected;
            });
        });

        if (!selected) {
            this.notificationService.error("You need to select some episodes!");
            return;
        }

        this.series.requested = true;

        const viewModel = <ITvRequestViewModel>{ firstSeason: this.series.firstSeason, latestSeason: this.series.latestSeason, requestAll: this.series.requestAll, tvDbId: this.series.id };
        viewModel.seasons = [];
        this.series.seasonRequests.forEach((season) => {
            const seasonsViewModel = <ISeasonsViewModel>{ seasonNumber: season.seasonNumber, episodes: [] };
            season.episodes.forEach(ep => {
                ep.requested = true;
                if (!this.series.latestSeason || !this.series.requestAll || !this.series.firstSeason) {
                    if (ep.selected) {
                        seasonsViewModel.episodes.push({ episodeNumber: ep.episodeNumber });
                    }
                }
            });

            viewModel.seasons.push(seasonsViewModel);
        });

        this.requestService.requestTv(viewModel)
            .subscribe(x => {
                if (x.result) {
                    this.notificationService.success(
                        `Request for ${this.series.title} has been added successfully`);

                    this.series.seasonRequests.forEach((season) => {
                        season.episodes.forEach((ep) => {
                            ep.selected = false;
                        });
                    });

                } else {
                    this.notificationService.warning("Request Added", x.errorMessage ? x.errorMessage : x.message);
                }
            });
    }

    public addRequest(episode: IEpisodesRequests) {
        episode.selected = true;
    }

    public removeRequest(episode: IEpisodesRequests) {
        episode.selected = false;
    }

    public seasonChanged(checkbox: MatCheckboxChange, season: INewSeasonRequests) {
        season.episodes.forEach((ep) => {
            if (checkbox.checked) {
                this.addRequest(ep)
            } else {
                this.removeRequest(ep);
            }
        });
    }
}
