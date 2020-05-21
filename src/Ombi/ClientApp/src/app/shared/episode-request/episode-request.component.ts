import { Component, OnInit, Inject } from "@angular/core";
import { MatCheckboxChange } from "@angular/material/checkbox";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { ISearchTvResultV2 } from "../../interfaces/ISearchTvResultV2";
import { RequestService, MessageService } from "../../services";
import { ITvRequestViewModel, ISeasonsViewModel, IEpisodesRequests, INewSeasonRequests } from "../../interfaces";


@Component({
    selector: "episode-request",
    templateUrl: "episode-request.component.html",
})
export class EpisodeRequestComponent implements OnInit {

    public loading: boolean;

    constructor(public dialogRef: MatDialogRef<EpisodeRequestComponent>, @Inject(MAT_DIALOG_DATA) public series: ISearchTvResultV2,
        private requestService: RequestService, private notificationService: MessageService) { }

    public ngOnInit() {
        this.loading = true;

        this.loading = false;
    }

    public async submitRequests() {
        // Make sure something has been selected
        const selected = this.series.seasonRequests.some((season) => {
            return season.episodes.some((ep) => {
                return ep.selected;
            });
        });
        debugger;
        if (!selected && !this.series.requestAll && !this.series.firstSeason && !this.series.latestSeason) {
            this.notificationService.send("You need to select some episodes!", "OK");
            return;
        }

        this.series.requested = true;

        const viewModel = <ITvRequestViewModel>{ firstSeason: this.series.firstSeason, latestSeason: this.series.latestSeason, requestAll: this.series.requestAll, tvDbId: this.series.id };
        viewModel.seasons = [];
        this.series.seasonRequests.forEach((season) => {
            const seasonsViewModel = <ISeasonsViewModel>{ seasonNumber: season.seasonNumber, episodes: [] };
            if (!this.series.latestSeason && !this.series.requestAll && !this.series.firstSeason) {
                season.episodes.forEach(ep => {
                    if (ep.selected) {
                        ep.requested = true;
                        seasonsViewModel.episodes.push({ episodeNumber: ep.episodeNumber });
                    }
                });
            }
            viewModel.seasons.push(seasonsViewModel);
        });

        const requestResult = await this.requestService.requestTv(viewModel).toPromise();

        if (requestResult.result) {
            this.notificationService.send(
                `Request for ${this.series.title} has been added successfully`);

            this.series.seasonRequests.forEach((season) => {
                season.episodes.forEach((ep) => {
                    ep.selected = false;
                });
            });

        } else {
            this.notificationService.send(requestResult.errorMessage ? requestResult.errorMessage : requestResult.message);
        }
        this.dialogRef.close();
    }

    public addRequest(episode: IEpisodesRequests) {
        episode.selected = true;
    }

    public removeRequest(episode: IEpisodesRequests) {
        episode.selected = false;
    }

    public seasonChanged(checkbox: MatCheckboxChange, season: INewSeasonRequests) {
        season.episodes.forEach((ep) => {
            if (checkbox.checked && (!ep.available && !ep.requested && !ep.approved)) {
                this.addRequest(ep)
            } else {
                this.removeRequest(ep);
            }
        });
    }

    public async requestAllSeasons() {
        this.series.requestAll = true;
        await this.submitRequests();
    }

    public async requestFirstSeason() {
        this.series.firstSeason = true;
        await this.submitRequests();
    }

    public async requestLatestSeason() {
        this.series.latestSeason = true;
        await this.submitRequests();
    }
}
