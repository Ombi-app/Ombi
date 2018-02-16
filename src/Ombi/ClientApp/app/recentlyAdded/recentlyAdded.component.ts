import { Component, OnInit } from "@angular/core";

import { RecentlyAddedService } from "../services/index";
import { IRecentlyAddedMovies, IRecentlyAddedRangeModel } from "./../interfaces";

@Component({
    templateUrl: "recentlyAdded.component.html",
})

export class RecentlyAddedComponent implements OnInit {
    public movies: IRecentlyAddedMovies[];
    public range: Date[];
  
    constructor(private recentlyAddedService: RecentlyAddedService) {}
     
    public ngOnInit() {
        const weekAgo = new Date();
        weekAgo.setDate(weekAgo.getDate() - 7);

        const today =new Date();
        const initModel = <IRecentlyAddedRangeModel>{from: weekAgo, to: today}; 
        this.recentlyAddedService.getRecentlyAddedMovies(initModel).subscribe(x => this.movies = x);
     }

     public close() {
         if(this.range.length < 2) {
             return;
         }
         if(!this.range[1]) {
             // If we do not have a second date then just set it to now
            this.range[1] = new Date();
         }
         const initModel = <IRecentlyAddedRangeModel>{from: this.range[0], to: this.range[1]}; 
         this.recentlyAddedService.getRecentlyAddedMovies(initModel).subscribe(x => this.movies = x);
     }
}
