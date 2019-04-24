import { Component, OnInit } from "@angular/core";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged } from "rxjs/operators";

import { AuthService } from "../auth/auth.service";
import { IRequestEngineResult, ISearchMovieResult, ISearchMovieResultContainer } from "../interfaces";
import { NotificationService, RequestService, SearchService } from "../services";

@Component({
    selector: "movie-search-grid",
    templateUrl: "./moviesearchgrid.component.html",
})
export class MovieSearchGridComponent implements OnInit {

    public searchText: string;
    public searchChanged: Subject<string> = new Subject<string>();
    public movieResults: ISearchMovieResult[];
    public movieResultGrid: ISearchMovieResultContainer[] = [];
    public result: IRequestEngineResult;
    public searchApplied = false;

    constructor(
        private searchService: SearchService, private requestService: RequestService,
        private notificationService: NotificationService, private authService: AuthService) {

        this.searchChanged.pipe(
            debounceTime(600), // Wait Xms afterthe last event before emitting last event
            distinctUntilChanged(), // only emit if value is different from previous value
        ).subscribe(x => {
            this.searchText = x as string;
            if (this.searchText === "") {
                this.clearResults();
                return;
            }
            this.searchService.searchMovie(this.searchText)
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
            result: false,
            errorMessage: "",
        };
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public request(searchResult: ISearchMovieResult) {
        searchResult.requested = true;
        searchResult.requestProcessing = true;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMovie")) {
            searchResult.approved = true;
        }

        try {
            this.requestService.requestMovie({ theMovieDbId: searchResult.id, languageCode: "en" })
                .subscribe(x => {
                    this.result = x;

                    if (this.result.result) {
                        this.notificationService.success(
                            `Request for ${searchResult.title} has been added successfully`);
                        searchResult.processed = true;
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
                this.processGrid(x);
                this.getExtaInfo();
            });
    }
    public nowPlayingMovies() {
        this.clearResults();
        this.searchService.nowPlayingMovies()
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    public topRatedMovies() {
        this.clearResults();
        this.searchService.topRatedMovies()
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }
    public upcomingMovies() {
        this.clearResults();
        this.searchService.upcomingMovies()
            .subscribe(x => {
                this.movieResults = x;
                this.getExtaInfo();
            });
    }

    private getExtaInfo() {
        this.movieResults.forEach((val) => {
            this.searchService.getMovieInformation(val.id)
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

    private processGrid(movies: ISearchMovieResult[]) {
        let container = <ISearchMovieResultContainer> { movies: [] };
        movies.forEach((movie, i) => {
            i++;
            if ((i % 4) === 0) {
                container.movies.push(movie);
                this.movieResultGrid.push(container);
                container = <ISearchMovieResultContainer> { movies: [] };
            } else {
                container.movies.push(movie);
            }
        });
        this.movieResultGrid.push(container);
    }

}
