import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from "@angular/core";
import { DiscoverOption, IDiscoverCardResult } from "../../interfaces";
import { ISearchMovieResult, ISearchTvResult, RequestType } from "../../../interfaces";
import { SearchV2Service } from "../../../services";
import { StorageService } from "../../../shared/storage/storage-service";
import { MatButtonToggleChange } from '@angular/material/button-toggle';
import { Carousel } from 'primeng/carousel';

export enum DiscoverType {
    Upcoming,
    Trending,
    Popular,
    RecentlyRequested,
    Seasonal,
}

@Component({
    selector: "carousel-list",
    templateUrl: "./carousel-list.component.html",
    styleUrls: ["./carousel-list.component.scss"],
})
export class CarouselListComponent implements OnInit {

    @Input() public discoverType: DiscoverType;
    @Input() public id: string;
    @Input() public isAdmin: boolean;
    @Output() public movieCount: EventEmitter<number> = new EventEmitter();
    @ViewChild('carousel', {static: false}) carousel: Carousel;

    public DiscoverOption = DiscoverOption;
    public discoverOptions: DiscoverOption = DiscoverOption.Combined;
    public discoverResults: IDiscoverCardResult[] = [];
    public movies: ISearchMovieResult[] = [];
    public tvShows: ISearchTvResult[] = [];
    public responsiveOptions: any;
    public RequestType = RequestType;
    public loadingFlag: boolean;
    public DiscoverType = DiscoverType;

    get mediaTypeStorageKey() {
        return "DiscoverOptions" + this.discoverType.toString();
    };
    private amountToLoad = 17;
    private currentlyLoaded = 0;

    constructor(private searchService: SearchV2Service,
        private storageService: StorageService) {
        this.responsiveOptions = [
            {
                breakpoint: '4000px',
                numVisible: 17,
                numScroll: 16
            },
            {
                breakpoint: '3800px',
                numVisible: 16,
                numScroll: 15
            },
            {
                breakpoint: '3600px',
                numVisible: 15,
                numScroll: 14
            },
            {
                breakpoint: '3400px',
                numVisible: 14,
                numScroll: 13
            },
            {
                breakpoint: '3200px',
                numVisible: 13,
                numScroll: 12
            },
            {
                breakpoint: '3000px',
                numVisible: 12,
                numScroll: 11
            },
            {
                breakpoint: '2800px',
                numVisible: 11,
                numScroll: 10
            },
            {
                breakpoint: '2600px',
                numVisible: 10,
                numScroll: 9
            },
            {
                breakpoint: '2400px',
                numVisible: 9,
                numScroll: 8
            },
            {
                breakpoint: '2200px',
                numVisible: 8,
                numScroll: 7
            },
            {
                breakpoint: '2000px',
                numVisible: 7,
                numScroll: 6
            },
            {
                breakpoint: '1800px',
                numVisible: 6,
                numScroll: 5
            },
            {
                breakpoint: '1650px',
                numVisible: 5,
                numScroll: 4
            },
            {
                breakpoint: '1500px',
                numVisible: 4,
                numScroll: 3
            },
            {
                breakpoint: '768px',
                numVisible: 3,
                numScroll: 2
            },
            {
                breakpoint: '660px',
                numVisible: 2,
                numScroll: 2
            },
            {
                breakpoint: '480px',
                numVisible: 1,
                numScroll: 1
            }
        ];
    }

    public async ngOnInit() {
        this.currentlyLoaded = 0;
        const localDiscoverOptions = +this.storageService.get(this.mediaTypeStorageKey);
        if (localDiscoverOptions) {
            this.discoverOptions = DiscoverOption[DiscoverOption[localDiscoverOptions]];
        }

        let currentIteration = 0;
        while (this.discoverResults.length <= 14 && currentIteration <= 3) {
            currentIteration++;
            await this.loadData(false);
        }
    }

    public async toggleChanged(event: MatButtonToggleChange) {
        await this.switchDiscoverMode(event.value);
    }

    public async newPage() {
        // Note this is using the internal carousel APIs
        // https://github.com/primefaces/primeng/blob/master/src/app/components/carousel/carousel.ts
        if (!this.carousel?.page) {
            return;
        }

        var end = this.carousel.page >= (this.carousel.totalDots() - 2) || this.carousel.totalDots() === 1;
        if (end) {
            var moviePromise: Promise<void>;
            var tvPromise: Promise<void>;
            switch (+this.discoverOptions) {
                case DiscoverOption.Combined:
                    moviePromise = this.loadMovies();
                    tvPromise = this.loadTv();
                    break;
                case DiscoverOption.Movie:
                    moviePromise = this.loadMovies();
                    break;
                case DiscoverOption.Tv:
                    tvPromise = this.loadTv();
                    break;
            }
            await moviePromise;
            await tvPromise;

            this.createModel();
        }
    }

    private async loadData(clearExisting: boolean = true) {
        var moviePromise: Promise<void>;
        var tvPromise: Promise<void>;
        switch (+this.discoverOptions) {
            case DiscoverOption.Combined:
                moviePromise = this.loadMovies();
                tvPromise = this.loadTv();
                break;
            case DiscoverOption.Movie:
                moviePromise = this.loadMovies();
                break;
            case DiscoverOption.Tv:
                tvPromise = this.loadTv();
                break;
        }
        await moviePromise;
        await tvPromise;
        this.createInitialModel(clearExisting);
    }

    private async switchDiscoverMode(newMode: DiscoverOption) {
        if (this.discoverOptions === newMode) {
            return;
        }
        this.loading();
        this.currentlyLoaded = 0;
        this.discoverOptions = +newMode;
        this.storageService.save(this.mediaTypeStorageKey, newMode.toString());
        await this.loadData();
        this.finishLoading();
    }

    private async loadMovies() {
        switch (this.discoverType) {
            case DiscoverType.Popular:
                this.movies = await this.searchService.popularMoviesByPage(this.currentlyLoaded, this.amountToLoad);
                break;
            case DiscoverType.Trending:
                this.movies = await this.searchService.nowPlayingMoviesByPage(this.currentlyLoaded, this.amountToLoad);
                break;
            case DiscoverType.Upcoming:
                this.movies = await this.searchService.upcomingMoviesByPage(this.currentlyLoaded, this.amountToLoad);
                break
            case DiscoverType.RecentlyRequested:
                this.movies = await this.searchService.recentlyRequestedMoviesByPage(this.currentlyLoaded, this.amountToLoad);
                break;
            case DiscoverType.Seasonal:
                this.movies = await this.searchService.seasonalMoviesByPage(this.currentlyLoaded, this.amountToLoad);
                break;
        }
        this.movieCount.emit(this.movies.length);
        this.currentlyLoaded += this.amountToLoad;
    }

    private async loadTv() {
        switch (this.discoverType) {
            case DiscoverType.Popular:
                this.tvShows = await this.searchService.popularTvByPage(this.currentlyLoaded, this.amountToLoad);
                break;
            case DiscoverType.Trending:
                this.tvShows = await this.searchService.trendingTvByPage(this.currentlyLoaded, this.amountToLoad);
                break;
            case DiscoverType.Upcoming:
                this.tvShows = await this.searchService.anticipatedTvByPage(this.currentlyLoaded, this.amountToLoad);
                break
            case DiscoverType.RecentlyRequested:
                // this.tvShows = await this.searchService.recentlyRequestedMoviesByPage(this.currentlyLoaded, this.amountToLoad); // TODO need to do some more mapping
                break;
        }
        this.currentlyLoaded += this.amountToLoad;
    }

    private createInitialModel(clearExisting: boolean = true) {
        if (clearExisting) {
            this.clear();
        }
        this.createModel();
    }

    private createModel() {
        const tempResults = <IDiscoverCardResult[]>[];

        switch (+this.discoverOptions) {
            case DiscoverOption.Combined:
                tempResults.push(...this.mapMovieModel());
                tempResults.push(...this.mapTvModel());
                this.shuffle(tempResults);
                break;
            case DiscoverOption.Movie:
                tempResults.push(...this.mapMovieModel());
                break;
            case DiscoverOption.Tv:
                tempResults.push(...this.mapTvModel());
                break;
        }

        this.discoverResults.push(...tempResults);
        this.carousel.ngAfterContentInit();

        this.finishLoading();
    }

    private mapMovieModel(): IDiscoverCardResult[] {
        const tempResults = <IDiscoverCardResult[]>[];
        this.movies.forEach(m => {
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

    private mapTvModel(): IDiscoverCardResult[] {
        const tempResults = <IDiscoverCardResult[]>[];
        this.tvShows.forEach(m => {
            tempResults.push({
                available: m.fullyAvailable,
                posterPath: m.backdropPath ? `https://image.tmdb.org/t/p/w500/${m.backdropPath}` : "../../../images/default_tv_poster.png",
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

    private clear() {
        this.discoverResults = [];
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

    private finishLoading() {
        this.loadingFlag = false;
    }


}
