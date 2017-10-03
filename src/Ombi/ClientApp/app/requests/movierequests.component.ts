import { Component, OnInit } from "@angular/core";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";
import { Subject } from "rxjs/Subject";

import { AuthService } from "../auth/auth.service";
import { NotificationService, RadarrService, RequestService } from "../services";

import { IMovieRequests, IRadarrProfile, IRadarrRootFolder } from "../interfaces";

@Component({
    selector: "movie-requests",
    templateUrl: "./movierequests.component.html",
})
export class MovieRequestsComponent implements OnInit {
    public movieRequests: IMovieRequests[];

    public searchChanged: Subject<string> = new Subject<string>();
    public searchText: string;

    public isAdmin: boolean;

    public radarrProfiles: IRadarrProfile[];
    public radarrRootFolders: IRadarrRootFolder[];

    private currentlyLoaded: number;
    private amountToLoad: number;

    constructor(private requestService: RequestService,
                private auth: AuthService,
                private notificationService: NotificationService,
                private radarrService: RadarrService) {
        this.searchChanged
            .debounceTime(600) // Wait Xms after the last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.resetSearch();
                    return;
                }
                this.requestService.searchMovieRequests(this.searchText)
                    .subscribe(m => this.movieRequests = m);
            });
    }

    public ngOnInit() {
        this.radarrService.getQualityProfilesFromSettings().subscribe(x => this.radarrProfiles = x);
        this.radarrService.getRootFoldersFromSettings().subscribe(x => this.radarrRootFolders = x);

        this.amountToLoad = 5;
        this.currentlyLoaded = 5;
        this.loadInit();
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
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
        this.approveRequest(request);
    }

    public deny(request: IMovieRequests) {
        request.approved = false;
        request.denied = true;
        this.updateRequest(request);
    }

    public selectRootFolder(searchResult: IMovieRequests, rootFolderSelected: IRadarrRootFolder) {
        searchResult.rootPathOverride = rootFolderSelected.id;
    }

    public selectQualityProfile(searchResult: IMovieRequests, profileSelected: IRadarrProfile) {
        searchResult.qualityOverride = profileSelected.id;
    }

    private loadRequests(amountToLoad: number, currentlyLoaded: number) {
        this.requestService.getMovieRequests(amountToLoad, currentlyLoaded + 1)
            .subscribe(x => {
                this.movieRequests.push.apply(this.movieRequests, x);
                this.currentlyLoaded = currentlyLoaded + amountToLoad;
            });
    }

    private updateRequest(request: IMovieRequests) {
        this.requestService.updateMovieRequest(request)
            .subscribe(x => request = x);
    }

    private approveRequest(request: IMovieRequests) {
        this.requestService.approveMovie(request)
            .subscribe(x => {

                if (x.requestAdded) {
                    this.notificationService.success("Request Approved",
                        `Request for ${request.title} has been approved successfully`);
                } else {
                    this.notificationService.warning("Request Approved", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
    }

    private loadInit() {
        this.requestService.getMovieRequests(this.amountToLoad, 0)
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
