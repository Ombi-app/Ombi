import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';


import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { RequestService } from '../services/request.service';

import { IRequestModel } from '../interfaces/IRequestModel';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './request.component.html',
    providers: [RequestService]
})
export class RequestComponent implements OnInit {
    constructor(private requestService: RequestService) {
        this.searchChanged
            .debounceTime(600) // Wait Xms afterthe last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.resetSearch();
                    return;
                }
                this.requestService.searchRequests(this.searchText).subscribe(x => this.requests = x);
            });
    }

    requests: IRequestModel[];

    searchChanged: Subject<string> = new Subject<string>();
    searchText: string;

    private currentlyLoaded: number;
    private amountToLoad : number;

    ngOnInit() {
        this.amountToLoad = 5;
        this.currentlyLoaded = 5;
        this.loadInit();
    }

    loadMore() {
        this.requestService.getRequests(this.amountToLoad, this.currentlyLoaded + 1).subscribe(x => {
            this.requests.push.apply(this.requests, x);
            this.currentlyLoaded = this.currentlyLoaded + this.amountToLoad;
        });
    }

    search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    removeRequest(request: IRequestModel) {
        this.requestService.removeRequest(request).subscribe();
        this.removeRequestFromUi(request);
    }

    changeAvailability(request: IRequestModel, available: boolean) {
        request.available = available;
        
        this.updateRequest(request);
    }

    approve(request: IRequestModel) {
        request.approved = true;
        request.denied = false;
        this.updateRequest(request);
    }

    deny(request: IRequestModel) {
        request.approved = false;
        request.denied = true;
        this.updateRequest(request);
    }

    private updateRequest(request: IRequestModel) {
        this.requestService.updateRequest(request).subscribe(x => request = x);
    }

    private loadInit() {
        this.requestService.getRequests(this.amountToLoad, 0).subscribe(x => this.requests = x);
    }

    private resetSearch() {
        this.currentlyLoaded = 5;
        this.loadInit();
    }

    private removeRequestFromUi(key : IRequestModel) {
        var index = this.requests.indexOf(key, 0);
        if (index > -1) {
            this.requests.splice(index, 1);
        }
    }
}