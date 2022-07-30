import { Component, Input } from "@angular/core";
import { IChildRequests, RequestSource, RequestType } from "../../../../../interfaces";

import { DenyDialogComponent } from "../../../shared/deny-dialog/deny-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { MessageService } from "../../../../../services";
import { RequestService } from "../../../../../services/request.service";
import { RequestServiceV2 } from "../../../../../services/requestV2.service";
import { TranslateService } from "@ngx-translate/core";

@Component({
    templateUrl: "./tv-requests-panel.component.html",
    styleUrls: ["./tv-requests-panel.component.scss"],
    selector: "tv-requests-panel"
})
export class TvRequestsPanelComponent {
    @Input() public tvRequest: IChildRequests[];
    @Input() public isAdmin: boolean;
    @Input() public manageOwnRequests: boolean;

    public RequestSource = RequestSource;

    public displayedColumns: string[] = ['number', 'title', 'airDate', 'status'];

    constructor(private requestService: RequestService,
        private requestService2: RequestServiceV2,
        private messageService: MessageService,
        public dialog: MatDialog, 
        private translateService: TranslateService) {

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
            this.messageService.send(this.translateService.instant("Requests.SuccessfullyApproved"));
        } else {
            this.messageService.sendRequestEngineResultError(result);
        }
    }

    public async delete(request: IChildRequests) {
        const result = await this.requestService.deleteChild(request.id).toPromise();

        if (result) {
            this.tvRequest.splice(this.tvRequest.indexOf(request),1);
            this.messageService.send(this.translateService.instant("Requests.SuccessfullyDeleted"));
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
                    this.messageService.send(
                        this.translateService.instant("Requests.NowAvailable"));
                } else {
                    this.messageService.sendRequestEngineResultError(x);
                    request.approved = false;
                }
            });
        } else {
            this.requestService.markTvUnavailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.messageService.send(
                        this.translateService.instant("Requests.NowUnavailable"));
                } else {
                    this.messageService.sendRequestEngineResultError(x);
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
