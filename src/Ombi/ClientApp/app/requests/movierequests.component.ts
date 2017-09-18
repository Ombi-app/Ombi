import { Component, OnDestroy, OnInit } from "@angular/core";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";
import "rxjs/add/operator/takeUntil";
import { Subject } from "rxjs/Subject";

import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";

import { AuthService } from "../auth/auth.service";
import { RequestService } from "../services";

import { IMovieRequests } from "../interfaces";

@Component({
    selector: "movie-requests",
    templateUrl: "./movierequests.component.html",
})
export class MovieRequestsComponent implements OnInit, OnDestroy {
    public movieRequests: IMovieRequests[];

    public searchChanged: Subject<string> = new Subject<string>();
    public searchText: string;

    public isAdmin: boolean;

    private currentlyLoaded: number;
    private amountToLoad: number;

    private subscriptions = new Subject<void>();

    constructor(private requestService: RequestService,
                private auth: AuthService) {
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
                this.requestService.searchMovieRequests(this.searchText)
                    .takeUntil(this.subscriptions)
                    .subscribe(m => this.movieRequests = m);
            });
    }

    public ngOnInit() {
        this.amountToLoad = 5;
        this.currentlyLoaded = 5;
        this.loadInit();
        this.isAdmin = this.auth.hasRole("admin");
    }

    public loadMore() {
        this.loadRequests(this.amountToLoad, this.currentlyLoaded);
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public removeRequest(request: IMovieRequests) {
        this.requestService.removeMovieRequest(request);
        this.removeRequestFromUi(request);
        this.loadRequests(1, this.currentlyLoaded);
    }

    public changeAvailability(request: IMovieRequests, available: boolean) {
        request.available = available;

        this.updateRequest(request);
    }

    public approve(request: IMovieRequests) {
        request.approved = true;
        request.denied = false;
        this.updateRequest(request);
    }

    public deny(request: IMovieRequests) {
        request.approved = false;
        request.denied = true;
        this.updateRequest(request);
    }

    public ngOnDestroy() {
        this.subscriptions.next();
        this.subscriptions.complete();
    }

    private loadRequests(amountToLoad: number, currentlyLoaded: number) {
        this.requestService.getMovieRequests(amountToLoad, currentlyLoaded + 1)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieRequests.push.apply(this.movieRequests, x);
                this.currentlyLoaded = currentlyLoaded + amountToLoad;
            });
    }

    private updateRequest(request: IMovieRequests) {
        this.requestService.updateMovieRequest(request)
            .takeUntil(this.subscriptions)
            .subscribe(x => request = x);
    }

    private loadInit() {
        this.requestService.getMovieRequests(this.amountToLoad, 0)
            .takeUntil(this.subscriptions)
            .subscribe(x => this.movieRequests = x);
    }

    private resetSearch() {
        this.currentlyLoaded = 5;
        this.loadInit();
    }

    private removeRequestFromUi(key: IMovieRequests) {
        const index = this.movieRequests.indexOf(key, 0);
        if (index > -1) {
            this.movieRequests.splice(index, 1);
        }
    }
}
