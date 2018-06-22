import { PlatformLocation } from "@angular/common";
import { Component, Input, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { TranslateService } from "@ngx-translate/core";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";
import { Subject } from "rxjs/Subject";

import { AuthService } from "../auth/auth.service";
import { IIssueCategory, IRequestEngineResult, ISearchMovieResult } from "../interfaces";
import { NotificationService, RequestService, SearchService } from "../services";

@Component({
    selector: "movie-search",
    templateUrl: "./moviesearch.component.html",
})
export class MovieSearchComponent implements OnInit {

    public searchText: string;
    public searchChanged: Subject<string> = new Subject<string>();
    public movieResults: ISearchMovieResult[];
    public result: IRequestEngineResult;
    public searchApplied = false;
    
    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    public issuesBarVisible = false;
    public issueRequestTitle: string;
    public issueRequestId: number;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;
    public defaultPoster: string;
        
    constructor(private searchService: SearchService, private requestService: RequestService,
                private notificationService: NotificationService, private authService: AuthService,
                private readonly translate: TranslateService, private sanitizer: DomSanitizer,
                private readonly platformLocation: PlatformLocation) {

        this.searchChanged
            .debounceTime(600) // Wait Xms after the last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.clearResults();
                    return;
                }
                this.searchService.searchMovie(this.searchText)
                    .subscribe(x => {
                        this.movieResults = x;
                        this.searchApplied = true;
                        // Now let's load some extra info including IMDB Id
                        // This way the search is fast at displaying results.
                        this.getExtraInfo();
                    });
            });
        this.defaultPoster = "../../../images/default_movie_poster.png";
        const base = this.platformLocation.getBaseHrefFromDOM();
        if (base) {
            this.defaultPoster = "../../.." + base + "/images/default_movie_poster.png";
        }
    }

    public ngOnInit() {
        this.searchText = "";
        this.movieResults = [];
        this.result = {
            message: "",
            result: false,
            errorMessage: "",
        };      
        this.popularMovies();
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public request(searchResult: ISearchMovieResult) {
        searchResult.requested = true;
        searchResult.requestProcessing = true;
        searchResult.showSubscribe = false;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMovie")) {
            searchResult.approved = true;
        }

        try {
            this.requestService.requestMovie({ theMovieDbId: searchResult.id })
                .subscribe(x => {
                    this.result = x;

                    if (this.result.result) {
                        this.translate.get("Search.RequestAdded", { title: searchResult.title }).subscribe(x => {
                            this.notificationService.success(x);
                            searchResult.processed = true;
                        });
                    } else {
                        if (this.result.errorMessage && this.result.message) {
                            this.notificationService.warning("Request Added", `${this.result.message} - ${this.result.errorMessage}`);
                        } else {
                            this.notificationService.warning("Request Added", this.result.message ? this.result.message : this.result.errorMessage);
                        }
                        searchResult.requested = false;
                        searchResult.approved = false;
                        searchResult.processed = false;
                        searchResult.requestProcessing = false;
                        
                    }
                });
        } catch (e) {

            searchResult.processed = false;
            searchResult.requestProcessing = false;
            this.notificationService.error(e);
        }
    }

    public popularMovies() {
        this.clearResults();
        this.searchService.popularMovies()
            .subscribe(x => {
                this.movieResults = x;
                this.getExtraInfo();
            });
    }
    public nowPlayingMovies() {
        this.clearResults();
        this.searchService.nowPlayingMovies()
            .subscribe(x => {
                this.movieResults = x;
                this.getExtraInfo();
            });
    }
    public topRatedMovies() {
        this.clearResults();
        this.searchService.topRatedMovies()
            .subscribe(x => {
                this.movieResults = x;
                this.getExtraInfo();
            });
    }
    public upcomingMovies() {
        this.clearResults();
        this.searchService.upcomingMovies()
            .subscribe(x => {
                this.movieResults = x;
                this.getExtraInfo();
            });
    }

    public reportIssue(catId: IIssueCategory, req: ISearchMovieResult) {
        this.issueRequestId = req.id;
        this.issueRequestTitle = req.title;
        this.issueCategorySelected = catId;
        this.issuesBarVisible = true;
        this.issueProviderId = req.id.toString();
    }

    public similarMovies(theMovieDbId: number) {
        this.clearResults();
        this.searchService.similarMovies(theMovieDbId)
        .subscribe(x => {
            this.movieResults = x;
            this.getExtraInfo();
        });
    }
        
    public subscribe(r: ISearchMovieResult) {
        r.subscribed = true;
        this.requestService.subscribeToMovie(r.requestId)
            .subscribe(x => {
                this.notificationService.success("Subscribed To Movie!");
            });
    }

    public unSubscribe(r: ISearchMovieResult) {
        r.subscribed = false;
        this.requestService.unSubscribeToMovie(r.requestId)
            .subscribe(x => {
                this.notificationService.success("Unsubscribed Movie!");
            });
    }

   private getExtraInfo() {

       this.movieResults.forEach((val, index) => {
           if (val.posterPath === null) {
               val.posterPath = this.defaultPoster;
           } else {
               val.posterPath = "https://image.tmdb.org/t/p/w300/" + val.posterPath;
           }
           val.background = this.sanitizer.bypassSecurityTrustStyle
           ("url(" + "https://image.tmdb.org/t/p/w1280" + val.backdropPath + ")");
           this.searchService.getMovieInformation(val.id)
                .subscribe(m => {
                    this.updateItem(val, m);
                });
        });
    }

    private updateItem(key: ISearchMovieResult, updated: ISearchMovieResult) {
        const index = this.movieResults.indexOf(key, 0);
        if (index > -1) {
            const copy = { ...this.movieResults[index] };
            this.movieResults[index] = updated;  
            this.movieResults[index].background = copy.background;
            this.movieResults[index].posterPath = copy.posterPath;
        }
    }
    private clearResults() {
        this.movieResults = [];
        this.searchApplied = false;
    }
}
