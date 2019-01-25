import { Component, OnInit } from "@angular/core";
import { SearchService } from "../../services";
import { ISearchMovieResult, ISearchTvResult } from "../../interfaces";

@Component({
    selector:"popular-movies",
    templateUrl: "./popular-movies.component.html",
})
export class PopularMoviesComponent implements OnInit {

    public movies: ISearchMovieResult[];

    constructor(private searchService: SearchService) {

    }
    public ngOnInit() {
        this.searchService.popularMovies().subscribe(x => this.movies = x);
    }
}
