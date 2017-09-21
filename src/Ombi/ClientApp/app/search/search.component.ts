import { Component, OnInit } from "@angular/core";
@Component({
    templateUrl: "./search.component.html",
})
export class SearchComponent implements OnInit  {
    public showTv: boolean;
    public showMovie: boolean;

    public ngOnInit() {
        this.showMovie = true;
        this.showTv = false;
    }

    public selectMovieTab() {
        this.showMovie = true;
        this.showTv = false;
    } 

    public selectTvTab() {
        this.showMovie = false;
        this.showTv = true;
    }
}
