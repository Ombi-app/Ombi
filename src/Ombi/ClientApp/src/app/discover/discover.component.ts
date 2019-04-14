import { Component, OnInit } from "@angular/core";
import { SearchV2Service } from "../services";
import { ISearchMovieResult, ISearchTvResult, RequestType } from "../interfaces";
import { IDiscoverCardResult } from "./interfaces";
import { trigger, transition, style, animate } from "@angular/animations";

@Component({
    templateUrl: "./discover.component.html",
    styleUrls: ["./discover.component.scss"],
    animations: [
        trigger('slideIn', [
            transition(':enter', [
              style({transform: 'translateX(100%)'}),
              animate('200ms ease-in', style({transform: 'translateY(0%)'}))
            ])
          ])
    ],
})
export class DiscoverComponent implements OnInit {

    public discoverResults: IDiscoverCardResult[] = [];
    public movies: ISearchMovieResult[];
    public tvShows: ISearchTvResult[];

    public defaultTvPoster: string;

    public popularActive: boolean = true;
    public trendingActive: boolean;
    public upcomingActive: boolean;

    public loadingFlag: boolean;

    private contentLoaded: number;

    constructor(private searchService: SearchV2Service) { }

    public async ngOnInit() {
        this.loading()

        this.movies = await this.searchService.popularMovies().toPromise();
        this.tvShows = await this.searchService.popularTv().toPromise();

        this.contentLoaded = 12;
                
        this.createModel(true);

    }

    public async onScroll() {
        console.log("SCROLLED!")
        this.movies = await this.searchService.popularMoviesByPage(this.contentLoaded, 12).toPromise();
        this.tvShows = [];
        this.contentLoaded+=12;
        
        this.createModel(false);
    }

    public async popular() {
        this.clear();
        
        this.contentLoaded = 12;
        this.loading()
        this.popularActive = true;
        this.trendingActive = false;
        this.upcomingActive = false;
        this.movies = await this.searchService.popularMovies().toPromise();
        this.tvShows = await this.searchService.popularTv().toPromise();

        
        this.createModel(true);
    }
    
    public async trending() {      
        this.clear();
        
        this.contentLoaded = 12;
        this.loading()
        this.popularActive = false;
        this.trendingActive = true;
        this.upcomingActive = false;
        this.movies = await this.searchService.nowPlayingMovies().toPromise();
        this.tvShows = await this.searchService.trendingTv().toPromise();

        this.createModel(true);
    }  

    public async upcoming() {    
        this.clear();
        this.contentLoaded = 12;
        this.loading()
        this.popularActive = false;
        this.trendingActive = false;
        this.upcomingActive = true;
        this.movies = await this.searchService.upcomingMovies().toPromise();
        this.tvShows = await this.searchService.anticipatedTv().toPromise();

        this.createModel(true);
    }

    private createModel(shuffle: boolean) {
        this.finishLoading();
        this.movies.forEach(m => {
            this.discoverResults.push({
                available: m.available,
                posterPath: `https://image.tmdb.org/t/p/w300/${m.posterPath}`,
                requested: m.requested,
                title: m.title,
                type: RequestType.movie,
                id: m.id,
                url: `http://www.imdb.com/title/${m.imdbId}/`,
                rating: m.voteAverage,
                overview: m.overview,
                approved: m.approved
            });
        });
        this.tvShows.forEach(m => {
            this.discoverResults.push({
                available: m.available,
                posterPath: "../../../images/default_tv_poster.png",
                requested: m.requested,
                title: m.title,
                type: RequestType.tvShow,
                id: m.id,
                url: undefined,
                rating: +m.rating,
                overview: m.overview,
                approved: m.approved
            });
        });
        if(shuffle) {
            this.shuffle(this.discoverResults);
        }
    }

    private shuffle(discover: IDiscoverCardResult[]) : IDiscoverCardResult[] {
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
