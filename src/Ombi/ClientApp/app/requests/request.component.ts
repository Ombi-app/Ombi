import { Component } from "@angular/core";

@Component({
    templateUrl: "./request.component.html",
})
export class RequestComponent  {

    public showMovie = true;
    public showTv = false;

    public selectTab() {
        this.showMovie = !this.showMovie;
        this.showTv = !this.showTv;
    }

}
