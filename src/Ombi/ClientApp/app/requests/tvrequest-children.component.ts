import { Component, Input } from "@angular/core";
import { IChildRequests, IEpisodesRequests } from "../interfaces";
import { NotificationService, RequestService } from "../services";

@Component({
    selector:"tvrequests-children",
    templateUrl: "./tvrequest-children.component.html",
})
export class TvRequestChildrenComponent {
    @Input() public childRequests: IChildRequests[];
    @Input() public isAdmin: boolean;
    constructor(private requestService: RequestService,
                private notificationService: NotificationService) { }

    public removeRequest(request: IChildRequests) {
        this.requestService.deleteChild(request)
            .subscribe();
        this.removeRequestFromUi(request);
    }

    public changeAvailability(request: IChildRequests, available: boolean) {
        request.available = available;
    }

    public deny(request: IChildRequests) {
        request.approved = false;
        request.denied = true;

        request.seasonRequests.forEach((season) => {
            season.episodes.forEach((ep) => {
                ep.approved = false;
            });
        });
        this.requestService.updateChild(request)
            .subscribe();
    }

    public approve(request: IChildRequests) {
        request.approved = true;
        request.denied = false;
        request.seasonRequests.forEach((season) => {
            season.episodes.forEach((ep) => {
                ep.approved = true;
            });
        });
        this.requestService.approveChild(request)
            .subscribe(x => {
                if (x.requestAdded) {
                    this.notificationService.success("Request Approved",
                        `Request has been approved successfully`);
                } else {
                    this.notificationService.warning("Request Approved", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
    }

    public denySeasonRequest(request: IChildRequests) {
        request.approved = false;
        request.denied = true;
        this.requestService.updateChild(request)
            .subscribe();
    }

    public getColour(ep: IEpisodesRequests): string {
        if (ep.available) {
            return "lime";
        }
        if (ep.approved) {
            return "#00c0ff";
        }
        return "white";
    }

    private removeRequestFromUi(key: IChildRequests) {
        const index = this.childRequests.indexOf(key, 0);
        if (index > -1) {
            this.childRequests.splice(index, 1);
        }
    }
}
