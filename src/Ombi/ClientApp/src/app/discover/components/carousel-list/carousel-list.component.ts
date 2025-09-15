import { Component, ViewChild, Inject, input, output, signal, computed, inject, ChangeDetectionStrategy } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatButtonToggleModule, MatButtonToggleChange } from '@angular/material/button-toggle';
import { TranslateModule } from "@ngx-translate/core";
import { CarouselModule, Carousel } from 'primeng/carousel';
import { SkeletonModule } from 'primeng/skeleton';

import { DiscoverOption, IDiscoverCardResult } from "../../interfaces";
import { ISearchMovieResult, ISearchTvResult, RequestType } from "../../../interfaces";
import { SearchV2Service } from "../../../services";
import { StorageService } from "../../../shared/storage/storage-service";
import { FeaturesFacade } from "../../../state/features/features.facade";
import { APP_BASE_HREF } from "@angular/common";
import { CarouselResponsiveOptions } from "../carousel.options";
import { DiscoverCardComponent } from "../card/discover-card.component";

export enum DiscoverType {
    Upcoming,
    Trending,
    Popular,
    RecentlyRequested,
    Seasonal,
}

@Component({
    standalone: true,
    selector: "carousel-list",
    templateUrl: "./carousel-list.component.html",
    styleUrls: ["./carousel-list.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CommonModule,
        MatButtonToggleModule,
        TranslateModule,
        CarouselModule,
        SkeletonModule,
        DiscoverCardComponent
    ]
})
export class CarouselListComponent {
    // Inputs using new input() function
    public discoverType = input.required<DiscoverType>();
    public id = input.required<string>();
    public isAdmin = input<boolean>(false);
    
    // Output using new output() function  
    public movieCount = output<number>();
    
    @ViewChild('carousel', {static: false}) carousel: Carousel;

    // Services using inject() function
    private searchService = inject(SearchV2Service);
    private storageService = inject(StorageService);
    private featureFacade = inject(FeaturesFacade);
    private baseUrl = inject(APP_BASE_HREF);

    // Public constants
    public DiscoverOption = DiscoverOption;
    public RequestType = RequestType;
    public DiscoverType = DiscoverType;
    
    // State using signals
    public discoverOptions = signal<DiscoverOption>(DiscoverOption.Combined);
    public discoverResults = signal<IDiscoverCardResult[]>([]);
    public movies = signal<ISearchMovieResult[]>([]);
    public tvShows = signal<ISearchTvResult[]>([]);
    public loadingFlag = signal<boolean>(false);
    public is4kEnabled = signal<boolean>(false);
    
    // Computed properties
    public hasResults = computed(() => this.discoverResults().length > 0);
    public totalResults = computed(() => this.discoverResults().length);

    get mediaTypeStorageKey() {
        return "DiscoverOptions" + this.discoverType().toString();
    };
    
    private amountToLoad = 10;
    private currentlyLoaded = 0;
    public responsiveOptions: any;

    constructor() {

        Carousel.prototype.onTouchMove = () => { },
        this.responsiveOptions = CarouselResponsiveOptions;
    }

    public async ngOnInit() {
        // Initialize 4K feature flag
        this.is4kEnabled.set(this.featureFacade.is4kEnabled());
        this.currentlyLoaded = 0;
        
        // Load saved discover options from storage
        const localDiscoverOptions = +this.storageService.get(this.mediaTypeStorageKey);
        if (localDiscoverOptions) {
            this.discoverOptions.set(DiscoverOption[DiscoverOption[localDiscoverOptions]]);
        }

        // Load initial data - just enough to fill the first carousel page
        // This reduces initial API calls and improves loading performance
        await this.loadData(false);
        
        // If we don't have enough results to fill the carousel, load one more batch
        if (this.discoverResults().length < 10) {
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
            switch (+this.discoverOptions()) {
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
        switch (+this.discoverOptions()) {
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
        if (this.discoverOptions() === newMode) {
            return;
        }
        this.loading();
        this.currentlyLoaded = 0;
        this.discoverOptions.set(+newMode);
        this.storageService.save(this.mediaTypeStorageKey, newMode.toString());
        await this.loadData();
        this.finishLoading();
    }

    private async loadMovies() {
        switch (this.discoverType()) {
            case DiscoverType.Popular:
                this.movies.set(await this.searchService.popularMoviesByPage(this.currentlyLoaded, this.amountToLoad));
                break;
            case DiscoverType.Trending:
                this.movies.set(await this.searchService.nowPlayingMoviesByPage(this.currentlyLoaded, this.amountToLoad));
                break;
            case DiscoverType.Upcoming:
                this.movies.set(await this.searchService.upcomingMoviesByPage(this.currentlyLoaded, this.amountToLoad));
                break;
            case DiscoverType.RecentlyRequested:
                this.movies.set(await this.searchService.recentlyRequestedMoviesByPage(this.currentlyLoaded, this.amountToLoad));
                break;
            case DiscoverType.Seasonal:
                this.movies.set(await this.searchService.seasonalMoviesByPage(this.currentlyLoaded, this.amountToLoad));
                break;
        }
        this.movieCount.emit(this.movies().length);
        this.currentlyLoaded += this.amountToLoad;
    }

    private async loadTv() {
        switch (this.discoverType()) {
            case DiscoverType.Popular:
                this.tvShows.set(await this.searchService.popularTvByPage(this.currentlyLoaded, this.amountToLoad));
                break;
            case DiscoverType.Trending:
                this.tvShows.set(await this.searchService.trendingTvByPage(this.currentlyLoaded, this.amountToLoad));
                break;
            case DiscoverType.Upcoming:
                this.tvShows.set(await this.searchService.anticipatedTvByPage(this.currentlyLoaded, this.amountToLoad));
                break;
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

        switch (+this.discoverOptions()) {
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

        this.discoverResults.update(current => [...current, ...tempResults]);

        this.finishLoading();
    }

    private mapMovieModel(): IDiscoverCardResult[] {
        const tempResults = <IDiscoverCardResult[]>[];
        this.movies().forEach(m => {
            tempResults.push({
                available: m.available,
                posterPath: m.posterPath ? `https://image.tmdb.org/t/p/w500/${m.posterPath}` : this.baseUrl + "/images/default_movie_poster.png",
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
        this.tvShows().forEach(m => {
            tempResults.push({
                available: m.fullyAvailable,
                posterPath: m.backdropPath ? `https://image.tmdb.org/t/p/w500/${m.backdropPath}` :  this.baseUrl + "/images/default_tv_poster.png",
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
        this.discoverResults.set([]);
    }

    private shuffle(discover: IDiscoverCardResult[]): IDiscoverCardResult[] {
        for (let i = discover.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [discover[i], discover[j]] = [discover[j], discover[i]];
        }
        return discover;
    }

    private loading() {
        this.loadingFlag.set(true);
    }

    private finishLoading() {
        this.loadingFlag.set(false);
    }


}
