import { Component } from "@angular/core";
import { ImageService, SearchV2Service } from "../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchMovieResultV2 } from "../interfaces/ISearchMovieResultV2";
import { MatDialog } from "@angular/material";
import { MovieDetailsTrailerComponent } from "./movie-details-trailer.component";

@Component({
    templateUrl: "./movie-details.component.html",
    styleUrls: ["./movie-details.component.scss"],
})
export class MovieDetailsComponent {
    public movie: ISearchMovieResultV2;
    private theMovidDbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
                private sanitizer: DomSanitizer, private imageService: ImageService,
                public dialog: MatDialog) {
        this.route.params.subscribe((params: any) => {
            this.theMovidDbId = params.movieDbId;
            this.load();
        });
    }

    public load() {
       this.searchService.getFullMovieDetails(this.theMovidDbId).subscribe(x => {
           this.movie = x;
            this.imageService.getMovieBanner(this.theMovidDbId.toString()).subscribe(x => {
                this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + x + ")");
            });
        });

    }

    public request() {
        //
    }

    public openDialog() {
        this.dialog.open(MovieDetailsTrailerComponent, {
          width: '560px',
          data: this.movie
        });
      }
}
