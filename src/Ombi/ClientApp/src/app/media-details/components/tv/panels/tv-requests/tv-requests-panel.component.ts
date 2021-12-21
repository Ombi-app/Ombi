import { Component, Input } from "@angular/core";
import { IChildRequests, RequestType } from "../../../../../interfaces";

import { DenyDialogComponent } from "../../../shared/deny-dialog/deny-dialog.component";
import { MatDialog } from "@angular/material/dialog";
import { MessageService } from "../../../../../services";
import { RequestService } from "../../../../../services/request.service";
import { RequestServiceV2 } from "../../../../../services/requestV2.service";

@Component({
    templateUrl: "./tv-requests-panel.component.html",
    styleUrls: ["./tv-requests-panel.component.scss"],
    selector: "tv-requests-panel"
})
export class TvRequestsPanelComponent {
    @Input() public tvRequest: IChildRequests[];
    @Input() public isAdmin: boolean;
    @Input() public manageOwnRequests: boolean;

    public displayedColumns: string[] = ['number', 'title', 'airDate', 'status'];

    constructor(private requestService: RequestService,
        private requestService2: RequestServiceV2,
        private messageService: MessageService,
        public dialog: MatDialog) {

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
            this.messageService.send("Request has been approved", "Ok");
        } else {
            this.messageService.sendRequestEngineResultError(result);
        }
    }

    public async delete(request: IChildRequests) {
        const result = await this.requestService.deleteChild(request.id).toPromise();

        if (result) {
            this.tvRequest.splice(this.tvRequest.indexOf(request),1);
            this.messageService.send("Request has been Deleted", "Ok");
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
                        `This request is now available`);
                } else {
                    this.messageService.send("Request Available", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
        } else {
            this.requestService.markTvUnavailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.messageService.send(
                    `This request is now unavailable`);
                } else {
                    this.messageService.send("Request Available", x.message ? x.message : x.errorMessage);
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
        this.requestService2.reprocessRequest(request.id, RequestType.tvShow).subscribe(result => {
            if (result.result) {
                this.messageService.send(result.message ? result.message : "Successfully Re-processed the request", "Ok");
            } else {
                this.messageService.sendRequestEngineResultError(result);
            }
        });
    }
}
