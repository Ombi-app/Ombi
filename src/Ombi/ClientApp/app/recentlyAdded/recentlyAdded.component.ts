import { Component, OnInit } from "@angular/core";

import { ImageService, RecentlyAddedService } from "../services";
import { IRecentlyAddedMovies, IRecentlyAddedRangeModel } from "./../interfaces";

@Component({
    templateUrl: "recentlyAdded.component.html",
})

export class RecentlyAddedComponent implements OnInit {
    public movies: IRecentlyAddedMovies[];
    public range: Date[];
  
    constructor(private recentlyAddedService: RecentlyAddedService,
                private imageService: ImageService) {}
     
    public ngOnInit() {
        const weekAgo = new Date();
        weekAgo.setDate(weekAgo.getDate() - 7);

        const today =new Date();
        const initModel = <IRecentlyAddedRangeModel>{from: weekAgo, to: today}; 
        this.recentlyAddedService.getRecentlyAddedMovies(initModel).subscribe(x => {
            this.movies = x;

            this.movies.forEach((movie) => {
                if(movie.theMovieDbId) {
                this.imageService.getMoviePoster(movie.theMovieDbId).subscribe(p => {
                    movie.posterPath = p;
                });
                } else {
                    movie.posterPath = "";
                }
            });
        });
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
     
     public page(event: any) {
         debugger;
         console.log(event);
     }
}
