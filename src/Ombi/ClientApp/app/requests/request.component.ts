import { Component } from "@angular/core";

@Component({
    templateUrl: "./request.component.html",
})
export class RequestComponent  {

    public showMovie = true;
    public showTv = false;

    public selectMovieTab() {
        this.showMovie = true;
        this.showTv = false;
    }

    public selectTvTab() {
        this.showMovie = false;
        this.showTv = true;
    }
}
