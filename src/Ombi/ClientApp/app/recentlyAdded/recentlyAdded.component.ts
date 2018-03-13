import { Component, OnInit } from "@angular/core";
import { NguCarousel } from "@ngu/carousel";

import { ImageService, RecentlyAddedService } from "../services";
import { IRecentlyAddedMovies, IRecentlyAddedRangeModel } from "./../interfaces";

@Component({
    templateUrl: "recentlyAdded.component.html",
    styles: [`
    .leftRs {
        position: absolute;
        margin: auto;
        top: 0;
        bottom: 0;
        width: 50px;
        height: 50px;
        box-shadow: 1px 2px 10px -1px rgba(0, 0, 0, .3);
        border-radius: 999px;
        left: 0;
    }

    .rightRs {
        position: absolute;
        margin: auto;
        top: 0;
        bottom: 0;
        width: 50px;
        height: 50px;
        box-shadow: 1px 2px 10px -1px rgba(0, 0, 0, .3);
        border-radius: 999px;
        right: 0;
    }
  `],
})

export class RecentlyAddedComponent implements OnInit {
    public movies: IRecentlyAddedMovies[];
    public range: Date[];
    
    // https://github.com/sheikalthaf/ngu-carousel
    public carouselTile: NguCarousel;
    
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
                } else if(movie.imdbId) {
                    this.imageService.getMoviePoster(movie.imdbId).subscribe(p => {
                        movie.posterPath = p;
                    });
                } else {
                    movie.posterPath = "";
                }
            });
        });

        this.carouselTile = {
          grid: {xs: 2, sm: 3, md: 3, lg: 5, all: 0},
          slide: 2,
          speed: 400,
          animation: "lazy",
          point: {
            visible: true,
          },
          load: 2,
          touch: true,
          easing: "ease",
        };
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
