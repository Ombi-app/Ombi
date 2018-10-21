import { PlatformLocation } from "@angular/common";
import { Component, Input, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";

import { AuthService } from "../auth/auth.service";
import { FilterType, IIssueCategory, IMovieRequests, IPagenator, IRadarrProfile, IRadarrRootFolder, OrderType } from "../interfaces";
import { NotificationService, RadarrService, RequestService } from "../services";
import { IRequestSearchModel } from "./search-bar/requests-search-bar.component";

@Component({
    selector: "movie-requests",
    templateUrl: "./movierequests.component.html",
})
export class MovieRequestsComponent implements OnInit {
    public movieRequests: IMovieRequests[];
    public defaultPoster: string;

    public isAdmin: boolean; // Also PowerUser

    public radarrProfiles: IRadarrProfile[];
    public radarrRootFolders: IRadarrRootFolder[];

    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    public issuesBarVisible = false;
    public issueRequest: IMovieRequests;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;

    public searchOptions: IRequestSearchModel = {
        searchText: "",
        filter: {
            availabilityFilter: FilterType.None,
            statusFilter: FilterType.None,
        },
        orderType: OrderType.RequestedDateDesc,
    };

    public totalMovies: number = 100;
    private currentlyLoaded: number;
    private amountToLoad: number;

    constructor(
        private requestService: RequestService,
        private auth: AuthService,
        private notificationService: NotificationService,
        private radarrService: RadarrService,
        private sanitizer: DomSanitizer,
        private readonly platformLocation: PlatformLocation) {
        this.defaultPoster = "../../../images/default_movie_poster.png";
        const base = this.platformLocation.getBaseHrefFromDOM();
        if (base) {
            this.defaultPoster = "../../.." + base + "/images/default_movie_poster.png";
        }
    }

    public ngOnInit() {
        this.amountToLoad = 10;
        this.currentlyLoaded = 10;
        this.loadInit();
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
    }

    public paginate(event: IPagenator) {
        const skipAmount = event.first;
        this.loadRequests(this.amountToLoad, skipAmount);
    }

    public onSearchEvent() {
        if (this.searchOptions.searchText === "") {
            this.resetSearch();
            return;
        }
        this.requestService.searchMovieRequests(this.searchOptions.searchText)
            .subscribe(m => {
                this.setOverrides(m);
                this.movieRequests = m;
            });
    }

    public removeRequest(request: IMovieRequests) {
        this.requestService.removeMovieRequest(request);
        this.removeRequestFromUi(request);
        this.loadRequests(this.amountToLoad, this.currentlyLoaded = 0);
    }

    public changeAvailability(request: IMovieRequests, available: boolean) {
        request.available = available;

        if (available) {
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
        this.denyRequest(request);
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

    public reportIssue(catId: IIssueCategory, req: IMovieRequests) {
        this.issueRequest = req;
        this.issueCategorySelected = catId;
        this.issuesBarVisible = true;
        this.issueProviderId = req.theMovieDbId.toString();
    }

    public ignore(event: any): void {
        event.preventDefault();
    }

    public subscribe(request: IMovieRequests) {
        request.subscribed = true;
        this.requestService.subscribeToMovie(request.id)
            .subscribe(x => {
                this.notificationService.success("Subscribed To Movie!");
            });
    }

    public unSubscribe(request: IMovieRequests) {
        request.subscribed = false;
        this.requestService.unSubscribeToMovie(request.id)
            .subscribe(x => {
                this.notificationService.success("Unsubscribed Movie!");
            });
    }

    public isRequestUser(request: IMovieRequests) {
        if (request.requestedUser.userName === this.auth.claims().name) {
            return true;
        }
        return false;
    }

    private loadRequests(amountToLoad: number, currentlyLoaded: number) {
        this.requestService.getMovieRequests(amountToLoad, currentlyLoaded, this.searchOptions.orderType, this.searchOptions.filter)
            .subscribe(x => {
                this.setOverrides(x.collection);
                if (!this.movieRequests) {
                    this.movieRequests = [];
                }
                this.movieRequests = x.collection;
                this.totalMovies = x.total;
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
                request.approved = true;
                if (x.result) {
                    this.notificationService.success(
                        `Request for ${request.title} has been approved successfully`);
                } else {
                    this.notificationService.warning("Request Approved", x.message ? x.message : x.errorMessage);
                    request.approved = false;
                }
            });
    }

    private denyRequest(request: IMovieRequests) {
        this.requestService.denyMovie({ id: request.id })
            .subscribe(x => {
                if (x.result) {
                    this.notificationService.success(
                        `Request for ${request.title} has been denied successfully`);
                } else {
                    this.notificationService.warning("Request Denied", x.message ? x.message : x.errorMessage);
                    request.denied = false;
                }
            });
    }

    private loadInit() {
        this.requestService.getMovieRequests(this.amountToLoad, 0, this.searchOptions.orderType, this.searchOptions.filter)
            .subscribe(x => {
                this.movieRequests = x.collection;
                this.totalMovies = x.total;

                this.movieRequests.forEach((req) => {
                    this.setBackground(req);
                    this.setPoster(req);
                });
                if (this.isAdmin) {
                    this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
                        this.radarrProfiles = c;
                        this.movieRequests.forEach((req) => this.setQualityOverrides(req));
                    });
                    this.radarrService.getRootFoldersFromSettings().subscribe(c => {
                        this.radarrRootFolders = c;
                        this.movieRequests.forEach((req) => this.setRootFolderOverrides(req));
                    });
                }
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
        this.setPoster(req);
        this.setBackground(req);
        this.setQualityOverrides(req);
        this.setRootFolderOverrides(req);
    }

    private setPoster(req: IMovieRequests): void {
        if (req.posterPath === null) {
            req.posterPath = this.defaultPoster;
        } else {
            req.posterPath = "https://image.tmdb.org/t/p/w300/" + req.posterPath;
        }
    }

    private setBackground(req: IMovieRequests): void {
        req.backgroundPath = this.sanitizer.bypassSecurityTrustStyle
            ("url(" + "https://image.tmdb.org/t/p/w1280" + req.background + ")");
    }

}
