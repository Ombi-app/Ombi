import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { TranslateModule } from "@ngx-translate/core";
import { IChildRequests, IEpisodesRequests, INewSeasonRequests, IRequestEngineResult, ISeasonsViewModel, ITvRequestViewModelV2, RequestType } from "../../../../../interfaces";
import { RequestService } from "../../../../../services/request.service";
import { MessageService } from "../../../../../services";
import { ISearchTvResultV2 } from "../../../../../interfaces/ISearchTvResultV2";
import { TranslateService } from "@ngx-translate/core";
import { MatDialog } from "@angular/material/dialog";
import { SelectionModel } from "@angular/cdk/collections";
import { RequestServiceV2 } from "../../../../../services/requestV2.service";
import { AdminRequestDialogComponent } from "../../../../../shared/admin-request-dialog/admin-request-dialog.component";
import { OmbiDatePipe } from "../../../../../pipes/OmbiDatePipe";
import { MatCardModule } from "@angular/material/card";

@Component({
    standalone: true,
    templateUrl: "./tv-request-grid.component.html",
    styleUrls: ["./tv-request-grid.component.scss"],
    selector: "tv-request-grid",
    imports: [
        CommonModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        MatCheckboxModule,
        MatSlideToggleModule,
        TranslateModule,
        OmbiDatePipe,
        MatCardModule,
    ]
})
export class TvRequestGridComponent {
    @Input() public tv: ISearchTvResultV2;
    @Input() public tvRequest: IChildRequests[];
    @Input() public isAdmin: boolean;
    public selection = new SelectionModel<IEpisodesRequests>(true, []);
    public selectedSeasonIndex: number = 0;

    public get requestable() {
        return this.tv?.seasonRequests?.length > 0;
    }

    public get selectedSeason(): INewSeasonRequests | null {
        if (!this.tv?.seasonRequests?.length) return null;
        return this.tv.seasonRequests[this.selectedSeasonIndex] || this.tv.seasonRequests[0];
    }

    public get selectionCount(): number {
        return this.selection.selected.length;
    }

    constructor(
        private requestService: RequestService,
        private requestServiceV2: RequestServiceV2,
        private notificationService: MessageService,
        private dialog: MatDialog,
        private translate: TranslateService
    ) {}

    public selectSeason(index: number) {
        this.selectedSeasonIndex = index;
    }

    public getCheckableEpisodes(season: INewSeasonRequests): IEpisodesRequests[] {
        return season.episodes.filter(ep => !ep.available && !ep.requested && !ep.approved && !ep.denied);
    }

    public isAllSeasonSelected(season: INewSeasonRequests): boolean {
        const checkable = this.getCheckableEpisodes(season);
        return checkable.length > 0 && checkable.every(ep => this.selection.isSelected(ep));
    }

    public isSomeSeasonSelected(season: INewSeasonRequests): boolean {
        const checkable = this.getCheckableEpisodes(season);
        return checkable.some(ep => this.selection.isSelected(ep)) && !this.isAllSeasonSelected(season);
    }

    public toggleSelectAll(season: INewSeasonRequests) {
        if (this.isAllSeasonSelected(season)) {
            this.getCheckableEpisodes(season).forEach(ep => this.selection.deselect(ep));
        } else {
            this.getCheckableEpisodes(season).forEach(ep => this.selection.select(ep));
        }
    }

    public getSeasonProgress(season: INewSeasonRequests): { available: number; requested: number; total: number } {
        const total = season.episodes.length;
        const available = season.episodes.filter(ep => ep.available).length;
        const requested = season.episodes.filter(ep => ep.requested || ep.approved).length;
        return { available, requested, total };
    }

    public getSeasonStatusClass(season: INewSeasonRequests): string {
        const progress = this.getSeasonProgress(season);
        if (progress.total === 0) return "";
        if (progress.available === progress.total) return "available";
        if (progress.available > 0 || progress.requested > 0) return "partial";
        const allDenied = season.episodes.every(ep => ep.denied);
        if (allDenied) return "denied";
        return "";
    }

    public isEpisodeDisabled(ep: IEpisodesRequests): boolean {
        return ep.available || ep.requested || ep.approved || ep.denied;
    }

    public getEpisodeStatusKey(ep: IEpisodesRequests): string {
        if (ep.available) return "available";
        if (ep.denied) return "denied";
        if (ep.approved) return "approved";
        if (ep.requested) return "requested";
        return "";
    }

    public isSeasonCheckable(season: INewSeasonRequests) {
        return this.getCheckableEpisodes(season).length > 0;
    }

    public async submitRequests() {
        const selected = this.selection.hasValue();
        if (!selected && !this.tv.requestAll && !this.tv.firstSeason && !this.tv.latestSeason) {
            this.notificationService.send(this.translate.instant("Requests.NeedToSelectEpisodes"));
            return;
        }

        this.tv.requested = true;

        const viewModel = <ITvRequestViewModelV2>{
            firstSeason: this.tv.firstSeason,
            latestSeason: this.tv.latestSeason,
            requestAll: this.tv.requestAll,
            theMovieDbId: this.tv.id,
            requestOnBehalf: null,
            languageCode: this.translate.currentLang,
        };
        viewModel.seasons = [];
        this.tv.seasonRequests.forEach((season) => {
            const seasonsViewModel = <ISeasonsViewModel>{ seasonNumber: season.seasonNumber, episodes: [] };
            if (!this.tv.latestSeason && !this.tv.requestAll && !this.tv.firstSeason) {
                season.episodes.forEach((ep) => {
                    if (this.selection.isSelected(ep)) {
                        ep.requested = true;
                        ep.requestStatus = "Common.PendingApproval";
                        seasonsViewModel.episodes.push({ episodeNumber: ep.episodeNumber });
                    }
                });
            }
            viewModel.seasons.push(seasonsViewModel);
        });

        if (this.isAdmin) {
            const dialog = this.dialog.open(AdminRequestDialogComponent, {
                width: "700px",
                data: { type: RequestType.tvShow, id: this.tv.id, is4k: null },
                panelClass: "modal-panel",
            });
            dialog.afterClosed().subscribe(async (result) => {
                if (result) {
                    viewModel.requestOnBehalf = result.username?.id;
                    viewModel.qualityPathOverride = result?.sonarrPathId;
                    viewModel.rootFolderOverride = result?.sonarrFolderId;
                    viewModel.languageProfile = result?.sonarrLanguageId;

                    const requestResult = await this.requestServiceV2.requestTv(viewModel).toPromise();
                    this.postRequest(requestResult);
                }
            });
        } else {
            const requestResult = await this.requestServiceV2.requestTv(viewModel).toPromise();
            this.postRequest(requestResult);
        }
    }

    public async requestAllSeasons() {
        this.tv.requestAll = true;
        await this.submitRequests();
    }

    public async requestFirstSeason() {
        this.tv.firstSeason = true;
        await this.submitRequests();
    }

    public async requestLatestSeason() {
        this.tv.latestSeason = true;
        await this.submitRequests();
    }

    private postRequest(requestResult: IRequestEngineResult) {
        if (requestResult.result) {
            this.notificationService.send(
                this.translate.instant("Requests.RequestAddedSuccessfully", { title: this.tv.title })
            );

            this.selection.clear();

            if (this.tv.firstSeason) {
                this.tv.seasonRequests[0].episodes.forEach((ep) => {
                    ep.requested = true;
                    ep.requestStatus = "Common.PendingApproval";
                });
            }
            if (this.tv.requestAll) {
                this.tv.seasonRequests.forEach((season) => {
                    season.episodes.forEach((ep) => {
                        ep.requested = true;
                        ep.requestStatus = "Common.PendingApproval";
                    });
                });
            }
            if (this.tv.latestSeason) {
                this.tv.seasonRequests[this.tv.seasonRequests.length - 1].episodes.forEach((ep) => {
                    ep.requested = true;
                    ep.requestStatus = "Common.PendingApproval";
                });
            }
        } else {
            this.notificationService.sendRequestEngineResultError(requestResult);
        }
    }
}
