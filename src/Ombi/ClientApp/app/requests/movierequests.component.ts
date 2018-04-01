import { Component, Input, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";
import { Subject } from "rxjs/Subject";

import { AuthService } from "../auth/auth.service";
import { NotificationService, RadarrService, RequestService } from "../services";

import { FilterType, IFilter, IIssueCategory, IMovieRequests, IRadarrProfile, IRadarrRootFolder } from "../interfaces";

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

    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    public issuesBarVisible = false;
    public issueRequest: IMovieRequests;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;

    public filterDisplay: boolean;
    public filter: IFilter;
    public filterType = FilterType;

    public order: string = "requestedDate";
    public reverse = false;

    private currentlyLoaded: number;
    private amountToLoad: number;

    constructor(private requestService: RequestService,
                private auth: AuthService,
                private notificationService: NotificationService,
                private radarrService: RadarrService,
                private sanitizer: DomSanitizer) {
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
                        this.setOverrides(m);
                        this.movieRequests = m;
                    });
            });
    }

    public ngOnInit() {
        this.amountToLoad = 100;
        this.currentlyLoaded = 100;
        this.loadInit();
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.filter = {
            availabilityFilter: FilterType.None,
            statusFilter: FilterType.None};
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

    public clearFilter() {
        this.filterDisplay = false;
        this.filter.availabilityFilter = FilterType.None;
        this.filter.statusFilter = FilterType.None;
        
        this.resetSearch();
    }

    public filterAvailability(filter: FilterType) {
        this.filter.availabilityFilter = filter;
        this.requestService.filterMovies(this.filter)
        .subscribe(x => {
            this.setOverrides(x);
            this.movieRequests = x;
        });
    }

    public filterStatus(filter: FilterType) {
        this.filter.statusFilter = filter;
        this.requestService.filterMovies(this.filter)
        .subscribe(x => {
            this.setOverrides(x);
            this.movieRequests = x;
        });
    }

    public setOrder(value: string, el: any) {
        el = el.toElement || el.relatedTarget || el.target || el.srcElement;

        const parent = el.parentElement;
        const previousFilter = parent.querySelector(".active");

        if (this.order === value) {
            this.reverse = !this.reverse;
        } else {
            previousFilter.className = "";
            el.className = "active";
        }
        this.order = value;
      }

    private loadRequests(amountToLoad: number, currentlyLoaded: number) {
        this.requestService.getMovieRequests(amountToLoad, currentlyLoaded + 1)
            .subscribe(x => {
                this.setOverrides(x);
                if(!this.movieRequests) {
                    this.movieRequests = [];
                }
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
        this.requestService.getMovieRequests(this.amountToLoad, 0)
            .subscribe(x => {
                this.movieRequests = x;

                this.movieRequests.forEach((req) => {
                    this.movieRequests.forEach((req) => this.setBackground(req));
                });
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
        this.setBackground(req);
        this.setQualityOverrides(req);
        this.setRootFolderOverrides(req);
    }

    private setBackground(req: IMovieRequests): void {
        req.backgroundPath = this.sanitizer.bypassSecurityTrustStyle
        ("url(" + "https://image.tmdb.org/t/p/w1280" + req.background + ")");
    }
}
