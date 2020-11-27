import { Component, Input } from "@angular/core";
import { IChildRequests, RequestType } from "../../../../../interfaces";
import { RequestService } from "../../../../../services/request.service";
import { MessageService } from "../../../../../services";
import { MatDialog } from "@angular/material/dialog";
import { DenyDialogComponent } from "../../../shared/deny-dialog/deny-dialog.component";

@Component({
    templateUrl: "./tv-requests-panel.component.html",
    styleUrls: ["./tv-requests-panel.component.scss"],
    selector: "tv-requests-panel"
})
export class TvRequestsPanelComponent {
    @Input() public tvRequest: IChildRequests[];
    @Input() public isAdmin: boolean;

    public displayedColumns: string[] = ['number', 'title', 'airDate', 'status'];

    constructor(private requestService: RequestService, private messageService: MessageService,
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
            this.messageService.send(result.errorMessage, "Ok");
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
}
