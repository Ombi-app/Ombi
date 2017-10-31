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

    public isAdmin: boolean; // Also PowerUser

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
                    .subscribe(m => {
                        this.movieRequests = m;
                    });
            });
    }

    public ngOnInit() {
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

        if(available) {
            this.requestService.markMovieAvailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.notificationService.success(
                        `${request.title} Is now available`);
                } else {
                    this.notificationService.warning("Request Available", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
        } else {
            this.requestService.markMovieUnavailable({ id: request.id }).subscribe(x => {
                if (x.result) {
                    this.notificationService.success(
                        `${request.title} Is now unavailable`);
                } else {
                    this.notificationService.warning("Request Available", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
        }
    }

    public approve(request: IMovieRequests) {
        request.approved = true;
        this.approveRequest(request);
    }

    public deny(request: IMovieRequests) {
        request.denied = true;
        this.updateRequest(request);
    }

    public selectRootFolder(searchResult: IMovieRequests, rootFolderSelected: IRadarrRootFolder, event: any) {
        event.preventDefault();
        searchResult.rootPathOverride = rootFolderSelected.id;
        this.setOverride(searchResult);
        this.updateRequest(searchResult);
    }

    public selectQualityProfile(searchResult: IMovieRequests, profileSelected: IRadarrProfile, event: any) {
        event.preventDefault();
        searchResult.qualityOverride = profileSelected.id;
        this.setOverride(searchResult);
        this.updateRequest(searchResult);
    }

    private loadRequests(amountToLoad: number, currentlyLoaded: number) {
        this.requestService.getMovieRequests(amountToLoad, currentlyLoaded + 1)
            .subscribe(x => {
                this.setOverrides(x);
                this.movieRequests.push.apply(this.movieRequests, x);
                this.currentlyLoaded = currentlyLoaded + amountToLoad;
            });
    }

    private updateRequest(request: IMovieRequests) {
        this.requestService.updateMovieRequest(request)
            .subscribe(x => {
                this.setOverride(x);
                request = x;
            });
    }

    private approveRequest(request: IMovieRequests) {
        this.requestService.approveMovie({ id: request.id })
            .subscribe(x => {

                if (x.result) {
                    this.notificationService.success(
                        `Request for ${request.title} has been approved successfully`);
                } else {
                    this.notificationService.warning("Request Approved", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
    }

    private loadInit() {
        this.requestService.getMovieRequests(this.amountToLoad, 0)
            .subscribe(x => {
                this.movieRequests = x;
                this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
                    this.radarrProfiles = c;
                    this.movieRequests.forEach((req) => this.setQualityOverrides(req));
                });
                this.radarrService.getRootFoldersFromSettings().subscribe(c => {
                    this.radarrRootFolders = c;
                    this.movieRequests.forEach((req) => this.setRootFolderOverrides(req));
                });
            });
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

    private setOverrides(requests: IMovieRequests[]): void {
        requests.forEach((req) => {
            this.setOverride(req);
        });
    }

    private setQualityOverrides(req: IMovieRequests): void {
        if (this.radarrProfiles) {
            const profile = this.radarrProfiles.filter((p) => {
                return p.id === req.qualityOverride;
            });
            if (profile.length > 0) {
                req.qualityOverrideTitle = profile[0].name;
            }
        }
    }
    private setRootFolderOverrides(req: IMovieRequests): void {
        if (this.radarrRootFolders) {
            const path = this.radarrRootFolders.filter((folder) => {
                return folder.id === req.rootPathOverride;
            });
            if (path.length > 0) {
                req.rootPathOverrideTitle = path[0].path;
            }
        }
    }

    private setOverride(req: IMovieRequests): void {
       this.setQualityOverrides(req);
       this.setRootFolderOverrides(req);
    }
}
