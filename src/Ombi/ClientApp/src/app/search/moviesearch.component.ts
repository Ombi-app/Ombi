import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Component, Input, OnInit, Inject } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { TranslateService } from "@ngx-translate/core";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged } from "rxjs/operators";

import { AuthService } from "../auth/auth.service";
import { IIssueCategory, ILanguageRefine, IRequestEngineResult, ISearchMovieResult } from "../interfaces";
import { NotificationService, RequestService, SearchService, SettingsService } from "../services";

import * as languageData from "../../other/iso-lang.json";

@Component({
    selector: "movie-search",
    templateUrl: "./moviesearch.component.html",
    styleUrls: ["./search.component.scss"],
})
export class MovieSearchComponent implements OnInit {

    public searchText: string;
    public searchChanged: Subject<string> = new Subject<string>();
    public movieRequested: Subject<void> = new Subject<void>();
    public movieResults: ISearchMovieResult[];
    public result: IRequestEngineResult;

    public searchApplied = false;
    public refineSearchEnabled = false;
    public searchYear?: number;
    public actorSearch: boolean;
    public selectedLanguage: string;
    public langauges: ILanguageRefine[];

    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    public issuesBarVisible = false;
    public issueRequestTitle: string;
    public issueRequestId: number;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;
    public defaultPoster: string;
    private href: string;

    constructor(
        private searchService: SearchService, private requestService: RequestService,
        private notificationService: NotificationService, private authService: AuthService,
        private readonly translate: TranslateService, private sanitizer: DomSanitizer,
         @Inject(APP_BASE_HREF) href:string, private settingsService: SettingsService) {
            this.href= href;
        this.langauges = <ILanguageRefine[]><any>languageData;
        this.searchChanged.pipe(
            debounceTime(600), // Wait Xms after the last event before emitting last event
            distinctUntilChanged(), // only emit if value is different from previous value
        ).subscribe(x => {
            this.searchText = x as string;
            this.runSearch();
        });
        this.defaultPoster = "../../../images/default_movie_poster.png";
        const base = this.href;
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
        this.settingsService.getDefaultLanguage().subscribe(x => this.selectedLanguage = x);
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
            const language = this.selectedLanguage && this.selectedLanguage.length > 0 ? this.selectedLanguage : "en";
            this.requestService.requestMovie({ theMovieDbId: searchResult.id, languageCode: language })
                .subscribe(x => {
                    this.result = x;
                    if (this.result.result) {
                        this.movieRequested.next();
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
        const releaseDate = new Date(req.releaseDate);
        this.issueRequestTitle = req.title + ` (${releaseDate.getFullYear()})`;
        this.issueCategorySelected = catId;
        this.issuesBarVisible = true;
        this.issueProviderId = req.id.toString();
    }

    public similarMovies(theMovieDbId: number) {
        this.clearResults();
        const lang = this.selectedLanguage && this.selectedLanguage.length > 0 ? this.selectedLanguage : "";
        this.searchService.similarMovies(theMovieDbId, lang)
            .subscribe(x => {
                this.movieResults = x;
                this.getExtraInfo();
            });
    }

    public subscribe(r: ISearchMovieResult) {
        r.subscribed = true;
        this.requestService.subscribeToMovie(r.requestId)
            .subscribe(x => {
                this.notificationService.success(`Subscribed To Movie ${r.title}!`);
            });
    }

    public unSubscribe(r: ISearchMovieResult) {
        r.subscribed = false;
        this.requestService.unSubscribeToMovie(r.requestId)
            .subscribe(x => {
                this.notificationService.success("Unsubscribed Movie!");
            });
    }

    public refineOpen() {
        this.refineSearchEnabled = !this.refineSearchEnabled;
        if (!this.refineSearchEnabled) {
            this.searchYear = undefined;
        }
    }

    public applyRefinedSearch() {
        this.runSearch();
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
            
            if (this.applyRefinedSearch) {
                this.searchService.getMovieInformationWithRefined(val.id, this.selectedLanguage)
                    .subscribe(m => {
                        this.updateItem(val, m);
                    });
            } else {
                this.searchService.getMovieInformation(val.id)
                    .subscribe(m => {
                        this.updateItem(val, m);
                    });
            }
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

    private runSearch() {
        if (this.searchText === "") {
            this.clearResults();
            return;
        }
        if (this.refineOpen) {
            if (!this.actorSearch) {
                this.searchService.searchMovieWithRefined(this.searchText, this.searchYear, this.selectedLanguage)
                    .subscribe(x => {
                        this.movieResults = x;
                        this.searchApplied = true;
                        // Now let's load some extra info including IMDB Id
                        // This way the search is fast at displaying results.
                        this.getExtraInfo();
                    });
            } else {
                this.searchService.searchMovieByActor(this.searchText, this.selectedLanguage)
                    .subscribe(x => {
                        this.movieResults = x;
                        this.searchApplied = true;
                        // Now let's load some extra info including IMDB Id
                        // This way the search is fast at displaying results.
                        this.getExtraInfo();
                    });
            }
        } else {
            this.searchService.searchMovie(this.searchText)
                .subscribe(x => {
                    this.movieResults = x;
                    this.searchApplied = true;
                    // Now let's load some extra info including IMDB Id
                    // This way the search is fast at displaying results.
                    this.getExtraInfo();
                });
        }
    }
}
