import { Component } from '@angular/core';

@Component({
    templateUrl: './request.component.html'
})
export class RequestComponent  {

    showMovie = true;
    showTv = false;

    selectTab() {
        this.showMovie = !this.showMovie;
        this.showTv = !this.showTv;
    }

}