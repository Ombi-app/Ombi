import { Component, OnInit } from "@angular/core";
import { SearchService } from "../services";
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
    private movies: ISearchMovieResult[];
    private tvShows: ISearchTvResult[];

    public defaultTvPoster: string;

    constructor(private searchService: SearchService) {

    }
    public async ngOnInit() {
        this.movies = await this.searchService.popularMovies().toPromise();
        this.tvShows = await this.searchService.popularTv().toPromise();

        this.movies.forEach(m => {
            this.discoverResults.push({
                available: m.available,
                posterPath: `https://image.tmdb.org/t/p/w300/${m.posterPath}`,
                requested: m.requested,
                title: m.title,
                type: RequestType.movie,
                id: m.id,
                url: `http://www.imdb.com/title/${m.imdbId}/`,
                rating: m.voteAverage
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
            });
        });

        this.shuffle(this.discoverResults);
    }

    private shuffle(discover: IDiscoverCardResult[]) : IDiscoverCardResult[] {
        for (let i = discover.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [discover[i], discover[j]] = [discover[j], discover[i]];
        }
        return discover;
    }
}
