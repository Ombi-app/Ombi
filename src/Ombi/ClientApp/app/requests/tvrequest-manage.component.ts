import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { RequestService } from '../services/request.service';
import { IdentityService } from '../services/identity.service';

import { IChildRequests } from '../interfaces/IRequestModel';

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
                    this.childRequests = x;
                });
            });

        this.isAdmin = this.identityService.hasRole('admin');
    }

    tvId: number;
    childRequests: IChildRequests[];
    isAdmin: boolean;

    public removeRequest(request: IChildRequests) {
        //this.requestService.removeTvRequest(request);
        this.removeRequestFromUi(request);
    }

    public changeAvailability(request: IChildRequests, available: boolean) {
        request.available = available;
    }

    public approve(request: IChildRequests) {
        request.approved = true;
        request.denied = false;
    }

    public deny(request: IChildRequests) {
        request.approved = false;
        request.denied = true;
    }

    public approveSeasonRequest(request: IChildRequests) {
        request.approved = true;
        request.denied = false;
        // this.requestService.updateTvRequest(this.selectedSeason)
        //     .subscribe();
    }

    public denySeasonRequest(request: IChildRequests) {
        request.approved = false;
        request.denied = true;
        // this.requestService.updateTvRequest(this.selectedSeason)
        //     .subscribe();
    }



    private removeRequestFromUi(key: IChildRequests) {
        var index = this.childRequests.indexOf(key, 0);
        if (index > -1) {
            this.childRequests.splice(index, 1);
        }
    }
}