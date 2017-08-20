import { Component, OnInit, OnDestroy, ViewEncapsulation } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';
import "rxjs/add/operator/takeUntil";

import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { RequestService } from '../services/request.service';
import { IdentityService } from '../services/identity.service';

import { ITvRequests, IChildRequests, INewSeasonRequests, IEpisodesRequests } from '../interfaces/IRequestModel';
import { TreeNode, } from "primeng/primeng";

@Component({
    selector: 'tv-requests',
    templateUrl: './tvrequests.component.html',
    styleUrls: ['./tvrequests.component.scss'],
    //Was required to turn off encapsulation since CSS only should be overridden for this component
    //However when encapsulation is on angular injects prefixes to all classes so css selectors
    //Stop working
    encapsulation: ViewEncapsulation.None
})
export class TvRequestsComponent implements OnInit, OnDestroy {
    constructor(private requestService: RequestService, private identityService: IdentityService) {
        this.searchChanged
            .debounceTime(600) // Wait Xms afterthe last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.resetSearch();
                    return;
                }
                this.requestService.searchTvRequests(this.searchText)
                    .takeUntil(this.subscriptions)
                    .subscribe(m => this.tvRequests = this.transformData(m));
            });
    }
    openClosestTab(el:any): void {
        var rowclass = "undefined";
        el = el.toElement;
        while (el.className != rowclass) {
            // Increment the loop to the parent node until we find the row we need
            el = el.parentNode;
            if (!el) {
            }
        }
        // At this point, the while loop has stopped and `el` represents the element that has
        // the class you specified

        // Then we loop through the children to find the caret which we want to click
        var caretright = "ui-treetable-toggler fa fa-fw ui-c fa-caret-right";
        var caretdown = "ui-treetable-toggler fa fa-fw ui-c fa-caret-down";
        for (var value of el.children) {
            // the caret from the ui has 2 class selectors depending on if expanded or not
            // we search for both since we want to still toggle the clicking
            if (value.className === caretright || value.className === caretdown )
            {
                // Then we tell JS to click the element even though we hid it from the UI
                value.click();
                //Break from loop since we no longer need to continue looking
                break;
            }
        };
    }
    transformData(datain: ITvRequests[]): any {
        var temp: TreeNode[] = [];
        datain.forEach(function (value) {
            temp.push({
                "data": value,
                "children": [{
                    "data": this.fixEpisodeSort(value.childRequests), leaf: true
                }],
                leaf: false
            });
        }, this)
        console.log(temp);
        return <TreeNode[]>temp;
    }
    private subscriptions = new Subject<void>();

    tvRequests: ITvRequests[];

    searchChanged = new Subject<string>();
    searchText: string;

    isAdmin: boolean;

    private currentlyLoaded: number;
    private amountToLoad: number;

    public showChildDialogue = false; // This is for the child modal popup
    public selectedSeason: ITvRequests;

    fixEpisodeSort(items: IChildRequests[]) {
        items.forEach(function (value) {
            value.seasonRequests.forEach(function (requests: INewSeasonRequests) {
                requests.episodes.sort(function (a: IEpisodesRequests, b: IEpisodesRequests) {
                    return a.episodeNumber - b.episodeNumber;
                })
            })
        })
        return items;
    }
    ngOnInit() {
        this.amountToLoad = 5;
        this.currentlyLoaded = 5;
        this.tvRequests = [];
        this.loadInit();
    }

    public loadMore() {
        this.requestService.getTvRequests(this.amountToLoad, this.currentlyLoaded + 1)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvRequests.push.apply(this.tvRequests, x);
                this.currentlyLoaded = this.currentlyLoaded + this.amountToLoad;
            });
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public removeRequest(request: ITvRequests) {
        this.requestService.removeTvRequest(request);
        this.removeRequestFromUi(request);
    }
    public removeChildRequest(request: IChildRequests) {
        this.requestService.deleteChild(request)
            .subscribe();
        this.removeChildRequestFromUi(request);
    }

    public changeAvailability(request: IChildRequests, available: boolean) {
        request.available = available;

        //this.updateRequest(request);
    }

    //Was already here but not sure what's using it...'
    //public approve(request: IChildRequests) {
    //    request.approved = true;
    //    request.denied = false;
    //    //this.updateRequest(request);
    //}
    public approve(request: IChildRequests) {
        request.approved = true;
        request.denied = false;
        this.requestService.updateChild(request)
            .subscribe();
    }
    //Was already here but not sure what's using it...'
    //public deny(request: IChildRequests) {
    //    request.approved = false;
    //    request.denied = true;
    //    //this.updateRequest(request);
    //}
    public deny(request: IChildRequests) {
        request.approved = false;
        request.denied = true;
        this.requestService.updateChild(request)
            .subscribe();
    }

    public approveSeasonRequest(request: IChildRequests) {
        request.approved = true;
        request.denied = false;
        this.requestService.updateTvRequest(this.selectedSeason)
            .subscribe();
    }

    public denySeasonRequest(request: IChildRequests) {
        request.approved = false;
        request.denied = true;
        this.requestService.updateTvRequest(this.selectedSeason)
            .subscribe();
    }
    public denyChildSeasonRequest(request: IChildRequests) {
        request.approved = false;
        request.denied = true;
        this.requestService.updateChild(request)
            .subscribe();
    }

    public showChildren(request: ITvRequests) {
        this.selectedSeason = request;
        this.showChildDialogue = true;
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

    //private updateRequest(request: ITvRequests) {
    //    this.requestService.updateTvRequest(request)
    //        .takeUntil(this.subscriptions)
    //        .subscribe(x => request = x);
    //}

    private loadInit() {
        this.requestService.getTvRequests(this.amountToLoad, 0)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvRequests = this.transformData(x);
            });
        this.isAdmin = this.identityService.hasRole("Admin");
    }

    private resetSearch() {
        this.currentlyLoaded = 5;
        this.loadInit();
    }

    private removeRequestFromUi(key: ITvRequests) {
        var index = this.tvRequests.indexOf(key, 0);
        if (index > -1) {
            this.tvRequests.splice(index, 1);
        }
    }
    private removeChildRequestFromUi(key: IChildRequests) {
        //var index = this.childRequests.indexOf(key, 0);
        //if (index > -1) {
        //    this.childRequests.splice(index, 1);
        //}
        //TODO FIX THIS
    }


    ngOnDestroy(): void {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}