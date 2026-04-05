import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { IChildRequests, IEpisodesRequests, INewSeasonRequests, RequestSource, RequestType } from "../../../../../interfaces";

import { DenyDialogComponent } from "../../../shared/deny-dialog/deny-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { MessageService } from "../../../../../services";
import { RequestService } from "../../../../../services/request.service";
import { RequestServiceV2 } from "../../../../../services/requestV2.service";
import { TranslateModule, TranslateService } from "@ngx-translate/core";
import { OmbiDatePipe } from "../../../../../pipes/OmbiDatePipe";
import { MatButtonModule } from "@angular/material/button";

@Component({
    templateUrl: "./tv-requests-panel.component.html",
    styleUrls: ["./tv-requests-panel.component.scss"],
    selector: "tv-requests-panel",
    imports: [
        CommonModule,
        TranslateModule,
        OmbiDatePipe,
        MatButtonModule,
    ]
})
export class TvRequestsPanelComponent {
    @Input() public tvRequest: IChildRequests[];
    @Input() public isAdmin: boolean;
    @Input() public manageOwnRequests: boolean;

    public RequestSource = RequestSource;
    public expandedRequests = new Set<number>();
    public selectedSeasons: Record<number, number> = {};

    constructor(
        private readonly requestService: RequestService,
        private readonly requestService2: RequestServiceV2,
        private readonly messageService: MessageService,
        public dialog: MatDialog,
        private readonly translateService: TranslateService
    ) {}

    public isExpanded(request: IChildRequests): boolean {
        return this.expandedRequests.has(request.id);
    }

    public toggleExpanded(request: IChildRequests): void {
        if (this.expandedRequests.has(request.id)) {
            this.expandedRequests.delete(request.id);
        } else {
            this.expandedRequests.add(request.id);
        }
    }

    public getSelectedSeasonIndex(request: IChildRequests): number {
        return this.selectedSeasons[request.id] ?? 0;
    }

    public setSelectedSeason(request: IChildRequests, index: number): void {
        this.selectedSeasons[request.id] = index;
    }

    public getSelectedSeason(request: IChildRequests): INewSeasonRequests | null {
        const index = this.getSelectedSeasonIndex(request);
        return request.seasonRequests?.[index] || null;
    }

    public getRequestStatusClass(request: IChildRequests): string {
        if (request.available) return "available";
        if (request.denied) return "denied";
        if (request.approved) return "approved";
        return "requested";
    }

    public getEpisodeStatusClass(ep: IEpisodesRequests, request: IChildRequests): string {
        if (ep.available) return "available";
        if (ep.denied || request.denied) return "denied";
        if (request.approved || ep.approved) return "approved";
        return "requested";
    }

    public async approve(request: IChildRequests) {
        const result = await this.requestService.approveChild({
            id: request.id
        }).toPromise();

        if (result?.result) {
            request.approved = true;
            request.denied = false;
            request.seasonRequests.forEach((season) => {
                season.episodes.forEach((ep) => {
                    ep.approved = true;
                    ep.denied = false;
                });
            });
            this.messageService.send(this.translateService.instant("Requests.SuccessfullyApproved"));
        } else if (result) {
            this.messageService.sendRequestEngineResultError(result);
        }
    }

    public async delete(request: IChildRequests) {
        const result = await this.requestService.deleteChild(request.id).toPromise();

        if (result) {
            const index = this.tvRequest.findIndex((r) => r.id === request.id);
            if (index !== -1) {
                this.tvRequest.splice(index, 1);
            }
            this.messageService.send(this.translateService.instant("Requests.SuccessfullyDeleted"));
        }
    }

    public changeAvailability(request: IChildRequests, available: boolean) {
        const updateAvailability = (value: boolean) => {
            request.available = value;
            request.seasonRequests.forEach((season) => {
                season.episodes.forEach((ep) => {
                    ep.available = value;
                });
            });
        };

        if (available) {
            this.requestService.markTvAvailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    updateAvailability(true);
                    this.messageService.send(this.translateService.instant("Requests.NowAvailable"));
                } else {
                    this.messageService.sendRequestEngineResultError(x);
                }
            });
        } else {
            this.requestService.markTvUnavailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    updateAvailability(false);
                    this.messageService.send(this.translateService.instant("Requests.NowUnavailable"));
                } else {
                    this.messageService.sendRequestEngineResultError(x);
                }
            });
        }
    }

    public async deny(request: IChildRequests) {
        const dialogRef = this.dialog.open(DenyDialogComponent, {
            width: '250px',
            data: { requestId: request.id, requestType: RequestType.tvShow }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                request.denied = true;
                request.seasonRequests.forEach((season) => {
                    season.episodes.forEach((ep) => {
                        ep.approved = false;
                    });
                });
            }
        });
    }

    public reProcessRequest(request: IChildRequests) {
        this.requestService2.reprocessRequest(request.id, RequestType.tvShow, false).subscribe(x => {
            if (x.result) {
                this.messageService.send(this.translateService.instant("Requests.SuccessfullyReprocessed"));
            } else {
                this.messageService.sendRequestEngineResultError(x);
            }
        });
    }
}
