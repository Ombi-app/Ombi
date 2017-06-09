import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';
import "rxjs/add/operator/takeUntil";

import { SearchService } from '../services/search.service';
import { RequestService } from '../services/request.service';
import { NotificationService } from '../services/notification.service';

import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';

@Component({
    selector: 'movie-search',
    templateUrl: './moviesearch.component.html',
})
export class MovieSearchComponent implements OnInit, OnDestroy {

    searchText: string;
    private subscriptions = new Subject<void>();
    searchChanged: Subject<string> = new Subject<string>();
    movieResults: ISearchMovieResult[];
    result: IRequestEngineResult;
    searchApplied = false;

    constructor(private searchService: SearchService, private requestService: RequestService, private notificationService: NotificationService) {
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

    ngOnInit(): void {
        this.searchText = "";
        this.movieResults = [];
        this.result = {
            message: "",
            requestAdded: false
        }
    }

    search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    request(searchResult: ISearchMovieResult) {
        searchResult.requested = true;
        this.requestService.requestMovie(searchResult)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.result = x;

                if (this.result.requestAdded) {
                    this.notificationService.success("Request Added",
                        `Request for ${searchResult.title} has been added successfully`);
                } else {
                    this.notificationService.warning("Request Added", this.result.message);
                }
            });
    }

    popularMovies() {
        this.clearResults();
        this.searchService.popularMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    nowPlayingMovies() {
        this.clearResults();
        this.searchService.nowPlayingMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    topRatedMovies() {
        this.clearResults();
        this.searchService.topRatedMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    upcomingMovies() {
        this.clearResults();
        this.searchService.upcomignMovies()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }

    private getExtaInfo() {
        this.searchService.extraInfo(this.movieResults)
            .takeUntil(this.subscriptions)
            .subscribe(m => this.movieResults = m);
    }

    private clearResults() {
        this.movieResults = [];
        this.searchApplied = false;
    }

    ngOnDestroy(): void {
        this.subscriptions.next();
        this.subscriptions.complete();
    }

}