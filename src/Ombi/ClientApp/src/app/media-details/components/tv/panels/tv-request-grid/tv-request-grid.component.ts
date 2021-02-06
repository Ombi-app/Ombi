import { Component, Input } from "@angular/core";
import { IChildRequests, IEpisodesRequests, INewSeasonRequests, ISeasonsViewModel, ITvRequestViewModel, RequestType } from "../../../../../interfaces";
import { RequestService } from "../../../../../services/request.service";
import { MessageService } from "../../../../../services";
import { DenyDialogComponent } from "../../../shared/deny-dialog/deny-dialog.component";
import { ISearchTvResultV2 } from "../../../../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material/dialog";
import { SelectionModel } from "@angular/cdk/collections";

@Component({
    templateUrl: "./tv-request-grid.component.html",
    styleUrls: ["./tv-request-grid.component.scss"],
    selector: "tv-request-grid"
})
export class TvRequestGridComponent {
    @Input() public tv: ISearchTvResultV2;
    @Input() public tvRequest: IChildRequests[];
    @Input() public isAdmin: boolean;
    public selection = new SelectionModel<IEpisodesRequests>(true, []);

    public get requestable() {
        return this.tv?.seasonRequests?.length > 0
    }

    public displayedColumns: string[] = ['select', 'number', 'title', 'airDate', 'status'];

    constructor(private requestService: RequestService, private notificationService: MessageService,
        private dialog: MatDialog) {

    }

    public async submitRequests() {
        // Make sure something has been selected
        const selected = this.selection.hasValue();
        if (!selected && !this.tv.requestAll && !this.tv.firstSeason && !this.tv.latestSeason) {
            this.notificationService.send("You need to select some episodes!", "OK");
            return;
        }

        this.tv.requested = true;

        const viewModel = <ITvRequestViewModel>{
            firstSeason: this.tv.firstSeason, latestSeason: this.tv.latestSeason, requestAll: this.tv.requestAll, tvDbId: this.tv.id,
            requestOnBehalf: null
        };
        viewModel.seasons = [];
        this.tv.seasonRequests.forEach((season) => {
            const seasonsViewModel = <ISeasonsViewModel>{ seasonNumber: season.seasonNumber, episodes: [] };
            if (!this.tv.latestSeason && !this.tv.requestAll && !this.tv.firstSeason) {
                season.episodes.forEach(ep => {
                    if (this.selection.isSelected(ep)) {
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
                `Request for ${this.tv.title} has been added successfully`);

            this.selection.clear();

        } else {
            this.notificationService.send(requestResult.errorMessage ? requestResult.errorMessage : requestResult.message);
        }
    }

    public async approve(request: IChildRequests) {
        const result = await this.requestService.approveChild({
            id: request.id
        }).toPromise();

        if (result.result) {
            request.approved = true;
            request.denied = false;
            request.seasonRequests.forEach((season) => {
                season.episodes.forEach((ep) => {
                    ep.approved = true;
                });
            });
            this.notificationService.send("Request has been approved", "Ok");
        } else {
            this.notificationService.send(result.errorMessage, "Ok");
        }
    }

    public changeAvailability(request: IChildRequests, available: boolean) {
        request.available = available;
        request.seasonRequests.forEach((season) => {
            season.episodes.forEach((ep) => {
                ep.available = available;
            });
        });
        if (available) {
            this.requestService.markTvAvailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.notificationService.send(
                        `This request is now available`);
                } else {
                    this.notificationService.send("Request Available", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
        } else {
            this.requestService.markTvUnavailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.notificationService.send(
                    `This request is now unavailable`);
                } else {
                    this.notificationService.send("Request Available", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
        }
    }
    public async deny(request: IChildRequests) {
        const dialogRef = this.dialog.open(DenyDialogComponent, {
            width: '250px',
            data: {requestId: request.id,  requestType: RequestType.tvShow}
          });

          dialogRef.afterClosed().subscribe(result => {
            request.denied = true;
            request.seasonRequests.forEach((season) => {
                season.episodes.forEach((ep) => {
                    ep.approved = false;
                });
            });
          });
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

      /** Whether the number of selected elements matches the total number of rows. */
  public isAllSelected(dataSource: IEpisodesRequests[]) {
    const numSelected = this.selection.selected.length;
    const numRows = dataSource.length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  public masterToggle(dataSource: IEpisodesRequests[]) {
    this.isAllSelected(dataSource) ?
        this.selection.clear() :
        dataSource.forEach(row => {
            if (!row.available && !row.requested && !row.approved) {
                this.selection.select(row)
            }
        });
  }

  public isSeasonCheckable(season: INewSeasonRequests) {
      const seasonAvailable = season.episodes.every((ep) => {
        return ep.available || ep.requested || ep.approved;
      });
      return !seasonAvailable;
    }

    public getStatusClass(season: INewSeasonRequests): string {
        const seasonAvailable = season.episodes.every((ep) => {
            return ep.available;
          });
        if (seasonAvailable) {
            return "available";
        }

        const seasonPending = season.episodes.some((ep) => {
            return ep.requested && !ep.approved
          });
        if (seasonPending) {
            return "requested";
        }

        const seasonApproved = season.episodes.some((ep) => {
            return ep.requested && ep.approved
          });
        if (seasonApproved) {
            return "approved";
        }
        return "";
    }

    public getEpisodeStatusClass(ep: IEpisodesRequests): string {
        if (ep.available) {
            return "available";
        }

        if (ep.requested && !ep.approved) {
            return "requested";
        }

        if (ep.requested && ep.approved) {
            return "approved";
        }
        return "";
    }
}
