import { Component, OnInit } from '@angular/core';
@Component({
    templateUrl: './search.component.html',
})
export class SearchComponent implements OnInit  {
    ngOnInit(): void {
        this.showMovie = true;
        this.showTv = false;
    }

    showTv : boolean;
    showMovie: boolean;


    selectTab() {
        this.showMovie = !this.showMovie;
        this.showTv = !this.showTv; 
    }

   
}