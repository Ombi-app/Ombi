import { Component, OnInit } from "@angular/core";
import { SearchService } from "../services";
import { ISearchMovieResult, ISearchTvResult } from "../interfaces";

@Component({
    templateUrl: "./home.component.html",
})
export class HomeComponent implements OnInit {

    public movies: ISearchMovieResult[];
    public tvShows: ISearchTvResult[];

    public defaultTvPoster: string;

    constructor(private searchService: SearchService) {

    }
    public ngOnInit() {
        this.defaultTvPoster = "../../../images/default_tv_poster.png";
        this.searchService.popularMovies().subscribe(x => this.movies = x);
        this.searchService.popularTv().subscribe(x => this.tvShows = x);
    }
}
