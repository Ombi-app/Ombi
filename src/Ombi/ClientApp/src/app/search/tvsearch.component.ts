import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Component, Input, OnInit, Inject } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged } from "rxjs/operators";

import { AuthService } from "../auth/auth.service";
import { IIssueCategory, IRequestEngineResult, ISearchTvResult, ISeasonsViewModel, ITvRequestViewModel } from "../interfaces";
import { ImageService, NotificationService, RequestService, SearchService } from "../services";

@Component({
    selector: "tv-search",
    templateUrl: "./tvsearch.component.html",
    styleUrls: ["./../requests/tvrequests.component.scss"],
})
export class TvSearchComponent implements OnInit {

    public searchText: string;
    public searchChanged = new Subject<string>();
    public tvResults: ISearchTvResult[];
    public tvRequested: Subject<void> = new Subject<void>();
    public result: IRequestEngineResult;
    public searchApplied = false;
    public defaultPoster: string;

    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    public issuesBarVisible = false;
    public issueRequestTitle: string;
    public issueRequestId: number;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;
    private href: string;

    constructor(
        private searchService: SearchService, private requestService: RequestService,
        private notificationService: NotificationService, private authService: AuthService,
        private imageService: ImageService, private sanitizer: DomSanitizer,
         @Inject(APP_BASE_HREF) href:string) {
this.href = href;
        this.searchChanged.pipe(
            debounceTime(600), // Wait Xms after the last event before emitting last event
            distinctUntilChanged(), // only emit if value is different from previous value
        ).subscribe(x => {
            this.searchText = x as string;
            if (this.searchText === "") {
                this.clearResults();
                return;
            }
            this.searchService.searchTv(this.searchText)
                .subscribe(x => {
                    this.tvResults = x;
                    this.searchApplied = true;
                    this.getExtraInfo();
                });
        });
        this.defaultPoster = "../../../images/default_tv_poster.png";
        const base = this.href;
        if (base) {
            this.defaultPoster = "../../.." + base + "/images/default_tv_poster.png";
        }
    }
    public openClosestTab(node: ISearchTvResult,el: any) {
        el.preventDefault();
        node.open = !node.open;
    }

    public ngOnInit() {
        this.searchText = "";
        this.tvResults = [];
        this.result = {
            message: "",
            result: false,
            errorMessage: "",
        };
        this.popularShows();
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public popularShows() {
        this.clearResults();
        this.searchService.popularTv()
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public trendingShows() {
        this.clearResults();
        this.searchService.trendingTv()
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public mostWatchedShows() {
        this.clearResults();
        this.searchService.mostWatchedTv()
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public anticipatedShows() {
        this.clearResults();
        this.searchService.anticipatedTv()
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public getExtraInfo() {
        this.tvResults.forEach((val, index) => {
            this.imageService.getTvBanner(val.id).subscribe(x => {
                if (x) {
                    val.background = this.sanitizer.
                        bypassSecurityTrustStyle
                        ("url(" + x + ")");
                }
            });
            this.searchService.getShowInformation(val.id)
                .subscribe(x => {
                    if (x) {
                        this.setDefaults(x);
                        this.updateItem(val, x);
                    } else {
                        const index = this.tvResults.indexOf(val, 0);
                        if (index > -1) {
                            this.tvResults.splice(index, 1);
                        }
                    }
                });
        });
    }

    public request(searchResult: ISearchTvResult) {
        searchResult.requested = true;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMovie")) {
            searchResult.approved = true;
        }

        const viewModel = <ITvRequestViewModel> { firstSeason: searchResult.firstSeason, latestSeason: searchResult.latestSeason, requestAll: searchResult.requestAll, tvDbId: searchResult.id };
        viewModel.seasons = [];
        searchResult.seasonRequests.forEach((season) => {
            const seasonsViewModel = <ISeasonsViewModel> { seasonNumber: season.seasonNumber, episodes: [] };
            season.episodes.forEach(ep => {
                if (!searchResult.latestSeason || !searchResult.requestAll || !searchResult.firstSeason) {
                    if (ep.requested) {
                        seasonsViewModel.episodes.push({ episodeNumber: ep.episodeNumber });
                    }
                }
            });

            viewModel.seasons.push(seasonsViewModel);
        });

        this.requestService.requestTv(viewModel)
            .subscribe(x => {
                this.tvRequested.next();
                this.result = x;
                if (this.result.result) {
                    this.notificationService.success(
                        `Request for ${searchResult.title} has been added successfully`);
                } else {
                    if (this.result.errorMessage && this.result.message) {
                        this.notificationService.warning("Request Added", `${this.result.message} - ${this.result.errorMessage}`);
                    } else {
                        this.notificationService.warning("Request Added", this.result.message ? this.result.message : this.result.errorMessage);
                    }
                }
            });
    }

    public allSeasons(searchResult: ISearchTvResult, event: any) {
        event.preventDefault();
        searchResult.requestAll = true;
        this.request(searchResult);
    }

    public firstSeason(searchResult: ISearchTvResult, event: any) {
        event.preventDefault();
        searchResult.firstSeason = true;
        this.request(searchResult);
    }

    public latestSeason(searchResult: ISearchTvResult, event: any) {
        event.preventDefault();
        searchResult.latestSeason = true;
        this.request(searchResult);
    }

    public reportIssue(catId: IIssueCategory, req: ISearchTvResult) {
        this.issueRequestId = req.id;
        const firstAiredDate = new Date(req.firstAired);
        this.issueRequestTitle = req.title + ` (${firstAiredDate.getFullYear()})`;
        this.issueCategorySelected = catId;
        this.issuesBarVisible = true;
        this.issueProviderId = req.id.toString();
    }

    private updateItem(key: ISearchTvResult, updated: ISearchTvResult) {
        const index = this.tvResults.indexOf(key, 0);
        if (index > -1) {
            // Update certain properties, otherwise we will loose some data
            this.tvResults[index].title = updated.title;
            this.tvResults[index].banner = updated.banner;
            this.tvResults[index].imdbId = updated.imdbId;
            this.tvResults[index].seasonRequests = updated.seasonRequests;
            this.tvResults[index].seriesId = updated.seriesId;
            this.tvResults[index].fullyAvailable = updated.fullyAvailable;
            this.tvResults[index].background = updated.banner;
        }
    }

    private setDefaults(x: ISearchTvResult) {
            if (x.banner === null) {
                x.banner = this.defaultPoster;
            }
        
            if (x.imdbId === null) {
                x.imdbId = "https://www.tvmaze.com/shows/" + x.seriesId;
            } else {
                x.imdbId = "http://www.imdb.com/title/" + x.imdbId + "/";
            }
    }

    private clearResults() {
        this.tvResults = [];
        this.searchApplied = false;
    }
}
