import { Component, OnInit, Inject } from "@angular/core";
import { MatCheckboxChange } from "@angular/material/checkbox";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { ISearchTvResultV2 } from "../../interfaces/ISearchTvResultV2";
import { RequestService, MessageService } from "../../services";
import { ITvRequestViewModel, ISeasonsViewModel, IEpisodesRequests, INewSeasonRequests } from "../../interfaces";
import { ThousandShortPipe } from "../../pipes/ThousandShortPipe";

export interface EpisodeRequestData {
    series: ISearchTvResultV2;
    requestOnBehalf: string | undefined;
}
@Component({
    selector: "episode-request",
    templateUrl: "episode-request.component.html",
})
export class EpisodeRequestComponent {

    public get requestable() {
        return this.data?.series?.seasonRequests?.length > 0
    }

    constructor(public dialogRef: MatDialogRef<EpisodeRequestComponent>, @Inject(MAT_DIALOG_DATA) public data: EpisodeRequestData,
        private requestService: RequestService, private notificationService: MessageService) { }


    public async submitRequests() {
        // Make sure something has been selected
        const selected = this.data.series.seasonRequests.some((season) => {
            return season.episodes.some((ep) => {
                return ep.selected;
            });
        });
        debugger;
        if (!selected && !this.data.series.requestAll && !this.data.series.firstSeason && !this.data.series.latestSeason) {
            this.notificationService.send("You need to select some episodes!", "OK");
            return;
        }

        this.data.series.requested = true;

        const viewModel = <ITvRequestViewModel>{
            firstSeason: this.data.series.firstSeason, latestSeason: this.data.series.latestSeason, requestAll: this.data.series.requestAll, tvDbId: this.data.series.id,
            requestOnBehalf: this.data.requestOnBehalf
        };
        viewModel.seasons = [];
        this.data.series.seasonRequests.forEach((season) => {
            const seasonsViewModel = <ISeasonsViewModel>{ seasonNumber: season.seasonNumber, episodes: [] };
            if (!this.data.series.latestSeason && !this.data.series.requestAll && !this.data.series.firstSeason) {
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
                `Request for ${this.data.series.title} has been added successfully`);

            this.data.series.seasonRequests.forEach((season) => {
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

    public isSeasonCheckable(season: INewSeasonRequests) {
        const seasonAvailable = season.episodes.every((ep) => {
          return ep.available || ep.requested || ep.approved;
        });
        return !seasonAvailable;
      }

    public async requestAllSeasons() {
        this.data.series.requestAll = true;
        await this.submitRequests();
    }

    public async requestFirstSeason() {
        this.data.series.firstSeason = true;
        await this.submitRequests();
    }

    public async requestLatestSeason() {
        this.data.series.latestSeason = true;
        await this.submitRequests();
    }
}
