import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { RequestService } from '../services/request.service';
import { IdentityService } from '../services/identity.service';

import { IChildRequests, IEpisodesRequests, INewSeasonRequests } from '../interfaces/IRequestModel';

@Component({
    templateUrl: './tvrequest-manage.component.html'
})
export class TvRequestManageComponent {
    constructor(private requestService: RequestService, private identityService: IdentityService,
        private route: ActivatedRoute) {

        this.route.params
            .subscribe(params => {
                this.tvId = +params['id']; // (+) converts string 'id' to a number
                this.requestService.getChildRequests(this.tvId).subscribe(x => {
                    this.childRequests = this.fixEpisodeSort(x);
                });
            });

        this.isAdmin = this.identityService.hasRole('admin');
    }

    tvId: number;
    childRequests: IChildRequests[];
    isAdmin: boolean;
    public fixEpisodeSort(items: IChildRequests[]) {
        items.forEach(function (value) {
            value.seasonRequests.forEach(function (requests: INewSeasonRequests) {
                requests.episodes.sort(function (a: IEpisodesRequests, b: IEpisodesRequests) {
                    return a.episodeNumber - b.episodeNumber;
                })
            })
        })
        return items;
    }
    public removeRequest(request: IChildRequests) {
        this.requestService.deleteChild(request)
            .subscribe();
        this.removeRequestFromUi(request);
    }

    public changeAvailability(request: IChildRequests, available: boolean) {
        request.available = available;
    }

    public deny(request: IChildRequests) {
        debugger;
        request.approved = false;
        request.denied = true;
        this.requestService.updateChild(request)
            .subscribe();
    }

    public approve(request: IChildRequests) {
        debugger;
        request.approved = true;
        request.denied = false;
        this.requestService.updateChild(request)
            .subscribe();
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
        var index = this.childRequests.indexOf(key, 0);
        if (index > -1) {
            this.childRequests.splice(index, 1);
        }
    }
}