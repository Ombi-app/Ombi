﻿import { Component, EventEmitter, Input, Output } from "@angular/core";
import { IChildRequests } from "../interfaces";
import { NotificationService, RequestService } from "../services";

@Component({
    selector:"tvrequests-children",
    templateUrl: "./tvrequest-children.component.html",
})
export class TvRequestChildrenComponent {
    @Input() public childRequests: IChildRequests[];
    @Input() public isAdmin: boolean;

    @Output() public requestDeleted = new EventEmitter<number>();

    constructor(private requestService: RequestService,
                private notificationService: NotificationService) { }

    public removeRequest(request: IChildRequests) {
        this.requestService.deleteChild(request)
            .subscribe(x => {
                this.removeRequestFromUi(request);
                this.requestDeleted.emit(request.id);
            });       
    }

    public changeAvailability(request: IChildRequests, available: boolean) {
        request.available = available;
        request.seasonRequests.forEach((season)=> {
            season.episodes.forEach((ep)=> {
                ep.available = available;
            });
        });
        if(available) {
            this.requestService.markTvAvailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.notificationService.success("Request Available",
                        `This request is now available`);
                } else {
                    this.notificationService.warning("Request Available", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
        } else {
            this.requestService.markTvUnavailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.notificationService.success("Request Available",
                    `This request is now unavailable`);
                } else {
                    this.notificationService.warning("Request Available", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
        }
    }

    public deny(request: IChildRequests) {
        request.denied = true;

        request.seasonRequests.forEach((season) => {
            season.episodes.forEach((ep) => {
                ep.approved = false;
            });
        });
        this.requestService.denyChild({ id: request.id })
            .subscribe(x => {
                if (x.result) {
                    this.notificationService.success("Request Denied",
                        `Request has been denied successfully`);
                } else {
                    this.notificationService.warning("Request Denied", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
    }

    public approve(request: IChildRequests) {
        request.approved = true;
        request.denied = false;
        request.seasonRequests.forEach((season) => {
            season.episodes.forEach((ep) => {
                ep.approved = true;
            });
        });
        this.requestService.approveChild({ id: request.id })
            .subscribe(x => {
                if (x.result) {
                    this.notificationService.success("Request Approved",
                        `Request has been approved successfully`);
                } else {
                    this.notificationService.warning("Request Approved", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
    }

    private removeRequestFromUi(key: IChildRequests) {
        const index = this.childRequests.indexOf(key, 0);
        if (index > -1) {
            this.childRequests.splice(index, 1);
        }
    }
}
