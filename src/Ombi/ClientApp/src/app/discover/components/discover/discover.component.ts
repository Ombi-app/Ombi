import { Component, OnInit } from "@angular/core";
import { SearchV2Service } from "../../../services";
import { ISearchMovieResult, ISearchTvResult, RequestType } from "../../../interfaces";
import { IDiscoverCardResult, DiscoverOption } from "../../interfaces";
import { trigger, transition, style, animate } from "@angular/animations";

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

    public discoverResults: IDiscoverCardResult[] = [];
    public movies: ISearchMovieResult[] = [];
    public tvShows: ISearchTvResult[] = [];
    
    public discoverOptions: DiscoverOption = DiscoverOption.Combined;
    public DiscoverOption = DiscoverOption;

    public defaultTvPoster: string;

    public popularActive: boolean = true;
    public trendingActive: boolean;
    public upcomingActive: boolean;

    public loadingFlag: boolean;
    public scrollDisabled: boolean;

    private contentLoaded: number;
    private isScrolling: boolean = false;

    constructor(private searchService: SearchV2Service) { }

    public async ngOnInit() {
        this.loading()
        this.scrollDisabled = true;
        switch (this.discoverOptions) {
            case DiscoverOption.Combined:
                this.movies = await this.searchService.popularMoviesByPage(0,12);
                this.tvShows = await this.searchService.popularTvByPage(0,12);
                break;
            case DiscoverOption.Movie:
                this.movies = await this.searchService.popularMoviesByPage(0,12);
                break;
            case DiscoverOption.Tv:
                this.tvShows = await this.searchService.popularTvByPage(0,12);
                break;
        }

        this.contentLoaded = 12;

        this.createInitialModel();
        this.scrollDisabled = false;
    }

    public async onScroll() {
        if (!this.contentLoaded) {
            return;
        }
        if (!this.isScrolling) {
            this.isScrolling = true;
            this.loading();
            if (this.popularActive) {
                switch (this.discoverOptions) {
                    case DiscoverOption.Combined:
                        this.movies = await this.searchService.popularMoviesByPage(this.contentLoaded, 12);
                        this.tvShows = await this.searchService.popularTvByPage(this.contentLoaded, 12);
                        break;
                    case DiscoverOption.Movie:
                        this.movies = await this.searchService.popularMoviesByPage(this.contentLoaded, 12);
                        break;
                    case DiscoverOption.Tv:
                        this.tvShows = await this.searchService.popularTvByPage(this.contentLoaded, 12);
                        break;
                }
            }
            if (this.trendingActive) {
                switch (this.discoverOptions) {
                    case DiscoverOption.Combined:
                        this.movies = await this.searchService.nowPlayingMoviesByPage(this.contentLoaded, 12);
                        this.tvShows = await this.searchService.trendingTvByPage(this.contentLoaded, 12);
                        break;
                    case DiscoverOption.Movie:
                        this.movies = await this.searchService.nowPlayingMoviesByPage(this.contentLoaded, 12);
                        break;
                    case DiscoverOption.Tv:
                        this.tvShows = await this.searchService.trendingTvByPage(this.contentLoaded, 12);
                        break;
                }
            }
            if (this.upcomingActive) {
                switch (this.discoverOptions) {
                    case DiscoverOption.Combined:
                        this.movies = await this.searchService.upcomingMoviesByPage(this.contentLoaded, 12);
                        this.tvShows = await this.searchService.anticipatedTvByPage(this.contentLoaded, 12);
                        break;
                    case DiscoverOption.Movie:
                        this.movies = await this.searchService.upcomingMoviesByPage(this.contentLoaded, 12);
                        break;
                    case DiscoverOption.Tv:
                        this.tvShows = await this.searchService.anticipatedTvByPage(this.contentLoaded, 12);
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
                this.movies = await this.searchService.popularMoviesByPage(0, 12);
                this.tvShows = await this.searchService.popularTvByPage(0, 12);
                break;
            case DiscoverOption.Movie:
                this.movies = await this.searchService.popularMoviesByPage(0, 12);
                break;
            case DiscoverOption.Tv:
                this.tvShows = await this.searchService.popularTvByPage(0, 12);
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
                this.movies = await this.searchService.nowPlayingMoviesByPage(0, 12);
                this.tvShows = await this.searchService.trendingTvByPage(0, 12);
                break;
            case DiscoverOption.Movie:
                this.movies = await this.searchService.nowPlayingMoviesByPage(0, 12);
                break;
            case DiscoverOption.Tv:
                this.tvShows = await this.searchService.trendingTvByPage(0, 12);
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
                this.movies = await this.searchService.upcomingMoviesByPage(0, 12);
                this.tvShows = await this.searchService.anticipatedTvByPage(0, 12);
                break;
            case DiscoverOption.Movie:
                this.movies = await this.searchService.upcomingMoviesByPage(0, 12);
                break;
            case DiscoverOption.Tv:
                this.tvShows = await this.searchService.anticipatedTvByPage(0, 12);
                break;
        }

        this.createModel();
        this.scrollDisabled = false;
    }

    public async switchDiscoverMode(newMode: DiscoverOption) {
        this.loading();
        this.clear();
        this.discoverOptions = newMode;
        await this.ngOnInit();
        this.finishLoading();
    }

    private createModel() {
        const tempResults = <IDiscoverCardResult[]>[];

        switch (this.discoverOptions) {
            case DiscoverOption.Combined:
                tempResults.push(...this.mapMovieModel());
                tempResults.push(...this.mapTvModel());
                break;
            case DiscoverOption.Movie:
                tempResults.push(...this.mapMovieModel());
                break;
            case DiscoverOption.Tv:
                tempResults.push(...this.mapTvModel());
                break;
        }

        this.shuffle(tempResults);
        this.discoverResults.push(...tempResults);

        this.finishLoading();
    }

    private mapMovieModel(): IDiscoverCardResult[] {
        const tempResults = <IDiscoverCardResult[]>[];
        this.movies.forEach(m => {
            tempResults.push({
                available: m.available,
                posterPath: m.posterPath ? `https://image.tmdb.org/t/p/w300/${m.posterPath}` : "../../../images/default_movie_poster.png",
                requested: m.requested,
                title: m.title,
                type: RequestType.movie,
                id: m.id,
                url: `http://www.imdb.com/title/${m.imdbId}/`,
                rating: m.voteAverage,
                overview: m.overview,
                approved: m.approved,
                imdbid: m.imdbId
            });
        });
        return tempResults;
    }

    private mapTvModel(): IDiscoverCardResult[] {
        const tempResults = <IDiscoverCardResult[]>[];
        this.tvShows.forEach(m => {
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
                approved: m.approved,
                imdbid: m.imdbId
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
}
