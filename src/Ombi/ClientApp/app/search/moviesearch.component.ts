import { Component, OnDestroy, OnInit } from "@angular/core";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";
import "rxjs/add/operator/takeUntil";
import { Subject } from "rxjs/Subject";

import { AuthService } from "../auth/auth.service";
import { NotificationService } from "../services";
import { RequestService } from "../services";
import { SearchService } from "../services";

import { IRequestEngineResult } from "../interfaces";
import { ISearchMovieResult } from "../interfaces";

@Component({
    selector: "movie-search",
    templateUrl: "./moviesearch.component.html",
})
export class MovieSearchComponent implements OnInit, OnDestroy {

    public searchText: string;
    public searchChanged: Subject<string> = new Subject<string>();
    public movieResults: ISearchMovieResult[];
    public result: IRequestEngineResult;
    public searchApplied = false;
    private subscriptions = new Subject<void>();

    constructor(private searchService: SearchService, private requestService: RequestService,
                private notificationService: NotificationService, private authService: AuthService) {

        this.searchChanged
            .debounceTime(600) // Wait Xms afterthe last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.clearResults();
                    return;
                }
                this.searchService.searchMovie(this.searchText)
                    .takeUntil(this.subscriptions)
                    .subscribe(x => {
                        this.movieResults = x;
                        this.searchApplied = true;
                        // Now let's load some exta info including IMDBId
                        // This way the search is fast at displaying results.
                        this.getExtaInfo();
                    });
            });
    }

    public ngOnInit() {
        this.searchText = "";
        this.movieResults = [];
        this.result = {
            message: "",
            requestAdded: false,
            errorMessage: "",
        };
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public request(searchResult: ISearchMovieResult) {
        searchResult.requested = true;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMovie")) {
            searchResult.approved = true;
        }

        this.requestService.requestMovie(searchResult)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.result = x;

                if (this.result.requestAdded) {
                    this.notificationService.success("Request Added",
                        `Request for ${searchResult.title} has been added successfully`);
                } else {
                    this.notificationService.warning("Request Added", this.result.message ? this.result.message : this.result.errorMessage);
                    searchResult.requested = false;
                    searchResult.approved = false;
                }
            });
    }

    public popularMovies() {
        this.clearResults();
        this.searchService.popularMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    public nowPlayingMovies() {
        this.clearResults();
        this.searchService.nowPlayingMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    public topRatedMovies() {
        this.clearResults();
        this.searchService.topRatedMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    public upcomingMovies() {
        this.clearResults();
        this.searchService.upcomignMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }

    public ngOnDestroy() {
        this.subscriptions.next();
        this.subscriptions.complete();
    }

    private getExtaInfo() {

        this.movieResults.forEach((val, index) => {
            this.searchService.getMovieInformation(val.id)
                .takeUntil(this.subscriptions)
                .subscribe(m => this.updateItem(val, m));
        });
    }

    private updateItem(key: ISearchMovieResult, updated: ISearchMovieResult) {
        const index = this.movieResults.indexOf(key, 0);
        if (index > -1) {
            this.movieResults[index] = updated;
        }
    }
    private clearResults() {
        this.movieResults = [];
        this.searchApplied = false;
    }
}
