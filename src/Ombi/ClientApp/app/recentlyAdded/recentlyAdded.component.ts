import { Component, OnInit } from "@angular/core";
import { NguCarouselConfig } from "@ngu/carousel";

import { IRecentlyAddedMovies, IRecentlyAddedTvShows } from "../interfaces";
import { ImageService, RecentlyAddedService } from "../services";

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
        border-radius: 100%;
        left: 0;
        background: #df691a;
    }

    .rightRs {
        position: absolute;
        margin: auto;
        top: 0;
        bottom: 0;
        width: 50px;
        height: 50px;
        box-shadow: 1px 2px 10px -1px rgba(0, 0, 0, .3);
        border-radius: 100%;
        right: 0;
        background: #df691a;
    }
  `],
})

export class RecentlyAddedComponent implements OnInit {
    public movies: IRecentlyAddedMovies[];
    public tv: IRecentlyAddedTvShows[];
    public range: Date[];

    public groupTv: boolean = false;

    // https://github.com/sheikalthaf/ngu-carousel
    public carouselTile: NguCarouselConfig;

    constructor(private recentlyAddedService: RecentlyAddedService,
                private imageService: ImageService) {}

    public ngOnInit() {
        this.getMovies();
        this.getShows();

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
         if (this.range.length < 2) {
             return;
         }
         if (!this.range[1]) {
             // If we do not have a second date then just set it to now
            this.range[1] = new Date();
         }
         this.getMovies();
     }

     public change() {
         this.getShows();
     }

     private getShows() {
         if (this.groupTv) {
            this.recentlyAddedService.getRecentlyAddedTvGrouped().subscribe(x => {
                this.tv = x;

                this.tv.forEach((t) => {
                    this.imageService.getTvPoster(t.tvDbId).subscribe(p => {
                        if (p) {
                            t.posterPath = p;
                        }
                    });
                });
            });
         } else {
            this.recentlyAddedService.getRecentlyAddedTv().subscribe(x => {
                this.tv = x;

                this.tv.forEach((t) => {
                    this.imageService.getTvPoster(t.tvDbId).subscribe(p => {
                        if (p) {
                            t.posterPath = p;
                        }
                    });
                });
            });
         }
     }

     private getMovies() {
        this.recentlyAddedService.getRecentlyAddedMovies().subscribe(x => {
            this.movies = x;

            this.movies.forEach((movie) => {
                if (movie.theMovieDbId) {
                this.imageService.getMoviePoster(movie.theMovieDbId).subscribe(p => {
                    movie.posterPath = p;
                });
                } else if (movie.imdbId) {
                    this.imageService.getMoviePoster(movie.imdbId).subscribe(p => {
                        movie.posterPath = p;
                    });
                } else {
                    movie.posterPath = "";
                }
            });
        });
     }
}
