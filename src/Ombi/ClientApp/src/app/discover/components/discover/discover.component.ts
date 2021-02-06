import { Component, OnInit, Inject } from "@angular/core";
import { SearchV2Service } from "../../../services";
import { ISearchMovieResult, ISearchTvResult, RequestType } from "../../../interfaces";
import { IDiscoverCardResult, DiscoverOption, DisplayOption } from "../../interfaces";
import { trigger, transition, style, animate } from "@angular/animations";
import { StorageService } from "../../../shared/storage/storage-service";
import { DOCUMENT } from "@angular/common";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { DiscoverType } from "../carousel-list/carousel-list.component";

@Component({
    templateUrl: "./discover.component.html",
    styleUrls: ["./discover.component.scss"],
    animations: [
        trigger('slideIn', [
            transition(':enter', [
                style({ transform: 'translateX(100%)' }),
                animate('200ms ease-in', style({ transform: 'translateY(0%)' }))
            ])
        ])
    ],
})
export class DiscoverComponent implements OnInit {

    public upcomingMovies: IDiscoverCardResult[] = [];
    public trendingMovies: IDiscoverCardResult[] = [];


    public discoverResults: IDiscoverCardResult[] = [];
    public movies: ISearchMovieResult[] = [];
    public tvShows: ISearchTvResult[] = [];

    public discoverOptions: DiscoverOption = DiscoverOption.Combined;
    public DiscoverType = DiscoverType;
    public DiscoverOption = DiscoverOption;
    public displayOption: DisplayOption = DisplayOption.Card;
    public DisplayOption = DisplayOption;

    public defaultTvPoster: string;

    public popularActive: boolean = true;
    public trendingActive: boolean;
    public upcomingActive: boolean;

    public loadingFlag: boolean;
    public scrollDisabled: boolean;

    private amountToLoad = 14;

    private contentLoaded: number;
    private isScrolling: boolean = false;
    private mediaTypeStorageKey = "DiscoverOptions";
    private displayOptionsKey = "DiscoverDisplayOptions";



    constructor(private searchService: SearchV2Service,
        private storageService: StorageService,
        @Inject(DOCUMENT) private container: Document) { }


    public async ngOnInit() {
        this.loading()
        // this.upcomingMovies = this.mapTvModel(await this.searchService.popularTvByPage(0, 14));
        // this.trendingMovies = this.mapMovieModel(await this.searchService.popularMoviesByPage(0, 14));
this.finishLoading();
        // const localDiscoverOptions = +this.storageService.get(this.mediaTypeStorageKey);
        // if (localDiscoverOptions) {
        //     this.discoverOptions = DiscoverOption[DiscoverOption[localDiscoverOptions]];
        // }
        // const localDisplayOptions = +this.storageService.get(this.displayOptionsKey);
        // if (localDisplayOptions) {
        //     this.displayOption = DisplayOption[DisplayOption[localDisplayOptions]];
        // }
        // this.scrollDisabled = true;
        // switch (this.discoverOptions) {
        //     case DiscoverOption.Combined:
        //         this.movies = await this.searchService.popularMoviesByPage(0, this.amountToLoad);
        //         this.tvShows = await this.searchService.popularTvByPage(0, this.amountToLoad);
        //         break;
        //     case DiscoverOption.Movie:
        //         this.movies = await this.searchService.popularMoviesByPage(0, this.amountToLoad);
        //         break;
        //     case DiscoverOption.Tv:
        //         this.tvShows = await this.searchService.popularTvByPage(0, this.amountToLoad);
        //         break;
        // }

        // this.contentLoaded = this.amountToLoad;

        // this.createInitialModel();
        // this.scrollDisabled = false;
        // if (!this.containerHasScrollBar()) {
        //     await this.onScroll();
        // }
    }

    public async onScroll() {
        console.log("scrolled");
        if (!this.contentLoaded) {
            return;
        }
        if (!this.isScrolling) {
            this.isScrolling = true;
            this.loading();
            if (this.popularActive) {
                switch (this.discoverOptions) {
                    case DiscoverOption.Combined:
                        this.movies = await this.searchService.popularMoviesByPage(this.contentLoaded, this.amountToLoad);
                        this.tvShows = await this.searchService.popularTvByPage(this.contentLoaded, this.amountToLoad);
                        break;
                    case DiscoverOption.Movie:
                        this.movies = await this.searchService.popularMoviesByPage(this.contentLoaded, this.amountToLoad);
                        break;
                    case DiscoverOption.Tv:
                        this.tvShows = await this.searchService.popularTvByPage(this.contentLoaded, this.amountToLoad);
                        break;
                }
            }
            if (this.trendingActive) {
                switch (this.discoverOptions) {
                    case DiscoverOption.Combined:
                        this.movies = await this.searchService.nowPlayingMoviesByPage(this.contentLoaded, this.amountToLoad);
                        this.tvShows = await this.searchService.trendingTvByPage(this.contentLoaded, this.amountToLoad);
                        break;
                    case DiscoverOption.Movie:
                        this.movies = await this.searchService.nowPlayingMoviesByPage(this.contentLoaded, this.amountToLoad);
                        break;
                    case DiscoverOption.Tv:
                        this.tvShows = await this.searchService.trendingTvByPage(this.contentLoaded, this.amountToLoad);
                        break;
                }
            }
            if (this.upcomingActive) {
                switch (this.discoverOptions) {
                    case DiscoverOption.Combined:
                        this.movies = await this.searchService.upcomingMoviesByPage(this.contentLoaded, this.amountToLoad);
                        this.tvShows = await this.searchService.anticipatedTvByPage(this.contentLoaded, this.amountToLoad);
                        break;
                    case DiscoverOption.Movie:
                        this.movies = await this.searchService.upcomingMoviesByPage(this.contentLoaded, this.amountToLoad);
                        break;
                    case DiscoverOption.Tv:
                        this.tvShows = await this.searchService.anticipatedTvByPage(this.contentLoaded, this.amountToLoad);
                        break;
                }
            }
            this.contentLoaded += 12;

            this.createModel();
            this.isScrolling = false;
        }
    }

    public async popular() {
        this.clear();
        this.scrollDisabled = true;
        this.isScrolling = false;
        this.contentLoaded = 12;
        this.loading()
        this.popularActive = true;
        this.trendingActive = false;
        this.upcomingActive = false;
        switch (this.discoverOptions) {
            case DiscoverOption.Combined:
                this.movies = await this.searchService.popularMoviesByPage(0, this.amountToLoad);
                this.tvShows = await this.searchService.popularTvByPage(0, this.amountToLoad);
                break;
            case DiscoverOption.Movie:
                this.movies = await this.searchService.popularMoviesByPage(0, this.amountToLoad);
                break;
            case DiscoverOption.Tv:
                this.tvShows = await this.searchService.popularTvByPage(0, this.amountToLoad);
                break;
        }

        this.createModel();
        this.scrollDisabled = false;
    }

    public async trending() {
        this.clear();

        this.scrollDisabled = true;
        this.isScrolling = false;
        this.contentLoaded = 12;
        this.loading()
        this.popularActive = false;
        this.trendingActive = true;
        this.upcomingActive = false;
        switch (this.discoverOptions) {
            case DiscoverOption.Combined:
                this.movies = await this.searchService.nowPlayingMoviesByPage(0, this.amountToLoad);
                this.tvShows = await this.searchService.trendingTvByPage(0, this.amountToLoad);
                break;
            case DiscoverOption.Movie:
                this.movies = await this.searchService.nowPlayingMoviesByPage(0, this.amountToLoad);
                break;
            case DiscoverOption.Tv:
                this.tvShows = await this.searchService.trendingTvByPage(0, this.amountToLoad);
                break;
        }

        this.createModel();
        this.scrollDisabled = false;
    }

    public async upcoming() {
        this.clear();
        this.scrollDisabled = true;
        this.isScrolling = false;
        this.contentLoaded = 12;
        this.loading()
        this.popularActive = false;
        this.trendingActive = false;
        this.upcomingActive = true;
        switch (this.discoverOptions) {
            case DiscoverOption.Combined:
                this.movies = await this.searchService.upcomingMoviesByPage(0, this.amountToLoad);
                this.tvShows = await this.searchService.anticipatedTvByPage(0, this.amountToLoad);
                break;
            case DiscoverOption.Movie:
                this.movies = await this.searchService.upcomingMoviesByPage(0, this.amountToLoad);
                break;
            case DiscoverOption.Tv:
                this.tvShows = await this.searchService.anticipatedTvByPage(0, this.amountToLoad);
                break;
        }

        this.createModel();
        this.scrollDisabled = false;
    }

    public async switchDiscoverMode(newMode: DiscoverOption) {
        this.loading();
        this.clear();
        this.discoverOptions = newMode;
        this.storageService.save(this.mediaTypeStorageKey, newMode.toString());
        await this.ngOnInit();
        this.finishLoading();
    }

    public changeView(view: DisplayOption) {
        this.displayOption = view;
        this.storageService.save(this.displayOptionsKey, view.toString());
    }

    private createModel() {
        const tempResults = <IDiscoverCardResult[]>[];

        // switch (this.discoverOptions) {
        //     case DiscoverOption.Combined:
        //         tempResults.push(...this.mapMovieModel());
        //         tempResults.push(...this.mapTvModel());
        //         break;
        //     case DiscoverOption.Movie:
        //         tempResults.push(...this.mapMovieModel());
        //         break;
        //     case DiscoverOption.Tv:
        //         tempResults.push(...this.mapTvModel());
        //         break;
        // }

        this.shuffle(tempResults);
        this.discoverResults.push(...tempResults);

        this.finishLoading();
    }

    private mapMovieModel(movies: ISearchMovieResult[]): IDiscoverCardResult[] {
        const tempResults = <IDiscoverCardResult[]>[];
        movies.forEach(m => {
            tempResults.push({
                available: m.available,
                posterPath: m.posterPath ? `https://image.tmdb.org/t/p/w500/${m.posterPath}` : "../../../images/default_movie_poster.png",
                requested: m.requested,
                title: m.title,
                type: RequestType.movie,
                id: m.id,
                url: `http://www.imdb.com/title/${m.imdbId}/`,
                rating: m.voteAverage,
                overview: m.overview,
                approved: m.approved,
                imdbid: m.imdbId,
                denied: false,
                background: m.backdropPath
            });
        });
        return tempResults;
    }

    private mapTvModel(tv: ISearchTvResult[]): IDiscoverCardResult[] {
        const tempResults = <IDiscoverCardResult[]>[];
        tv.forEach(m => {
            tempResults.push({
                available: m.available,
                posterPath: "../../../images/default_tv_poster.png",
                requested: m.requested,
                title: m.title,
                type: RequestType.tvShow,
                id: m.id,
                url: undefined,
                rating: +m.rating,
                overview: m.overview,
                approved: m.approved || m.partlyAvailable,
                imdbid: m.imdbId,
                denied: false,
                background: m.background
            });
        });
        return tempResults;
    }

    private createInitialModel() {
        this.clear();
        this.createModel();
    }

    private shuffle(discover: IDiscoverCardResult[]): IDiscoverCardResult[] {
        for (let i = discover.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [discover[i], discover[j]] = [discover[j], discover[i]];
        }
        return discover;
    }

    private loading() {
        this.loadingFlag = true;
    }

    private clear() {
        this.discoverResults = [];
    }

    private finishLoading() {
        this.loadingFlag = false;
    }

    private containerHasScrollBar(): boolean {
        return this.container.documentElement.scrollHeight > this.container.documentElement.clientHeight;
        // div.scrollHeight > div.clientHeight;
        // this.container.documentElement.scrollHeight > (window.innerHeight + window.pageYOffset);
    }
}
