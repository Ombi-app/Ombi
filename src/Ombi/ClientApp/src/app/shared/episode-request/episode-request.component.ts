import { Component, Inject } from "@angular/core";
import { MatCheckboxChange } from "@angular/material/checkbox";
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { ISearchTvResultV2 } from "../../interfaces/ISearchTvResultV2";
import { MessageService } from "../../services";
import { TranslateService } from "@ngx-translate/core";
import { ISeasonsViewModel, IEpisodesRequests, INewSeasonRequests, ITvRequestViewModelV2, IRequestEngineResult, RequestType } from "../../interfaces";
import { RequestServiceV2 } from "../../services/requestV2.service";
import { AdminRequestDialogComponent } from "../admin-request-dialog/admin-request-dialog.component";

export interface EpisodeRequestData {
    series: ISearchTvResultV2;
    isAdmin: boolean;
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
        private requestService: RequestServiceV2, private notificationService: MessageService, private dialog: MatDialog, 
        private translate: TranslateService) { }


    public async submitRequests() {
        // Make sure something has been selected
        const selected = this.data.series.seasonRequests.some((season) => {
            return season.episodes.some((ep) => {
                return ep.selected;
            });
        });

        if (!selected && !this.data.series.requestAll && !this.data.series.firstSeason && !this.data.series.latestSeason) {
            this.notificationService.send(this.translate.instant("Requests.NeedToSelectEpisodes"), "OK");
            return;
        }

        this.data.series.requested = true;

        const viewModel = <ITvRequestViewModelV2>{
            firstSeason: this.data.series.firstSeason, latestSeason: this.data.series.latestSeason, requestAll: this.data.series.requestAll, theMovieDbId: this.data.series.id,
            requestOnBehalf: this.data.requestOnBehalf, languageCode: this.translate.currentLang
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

        if (this.data.isAdmin) {
            const dialog = this.dialog.open(AdminRequestDialogComponent, { width: "700px", data: { type: RequestType.tvShow, id: this.data.series.id, is4k: null }, panelClass: 'modal-panel' });
            dialog.afterClosed().subscribe(async (result) => {
                if (result) {
                    viewModel.requestOnBehalf = result.username?.id;
                    viewModel.qualityPathOverride = result?.sonarrPathId;
                    viewModel.rootFolderOverride = result?.sonarrFolderId;
                    viewModel.languageProfile = result?.sonarrLanguageId;

                    const requestResult = await this.requestService.requestTv(viewModel).toPromise();
                    this.postRequest(requestResult);
                }
            });
        } else {
            const requestResult = await this.requestService.requestTv(viewModel).toPromise();
            this.postRequest(requestResult);
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

    private postRequest(requestResult: IRequestEngineResult) {
        if (requestResult.result) {
            this.notificationService.send(
                this.translate.instant("Requests.RequestAddedSuccessfully", { title: this.data.series.title }));

            this.data.series.seasonRequests.forEach((season) => {
                season.episodes.forEach((ep) => {
                    ep.selected = false;
                });
            });

        } else {
            this.notificationService.sendRequestEngineResultError(requestResult);
        }
    }
}
