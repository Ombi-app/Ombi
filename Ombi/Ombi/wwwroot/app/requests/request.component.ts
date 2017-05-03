import { Component } from '@angular/core';

@Component({
    selector: 'ombi',
    moduleId: module.id,
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