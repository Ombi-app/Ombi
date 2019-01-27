import { Component } from "@angular/core";
import { SearchService, ImageService } from "../services";
import { ActivatedRoute } from "@angular/router";
import { ISearchMovieResult } from "../interfaces";
import { DomSanitizer } from "@angular/platform-browser";

@Component({
    templateUrl: "./movie-details.component.html",
    styleUrls: ["./movie-details.component.scss"],
})
export class MovieDetailsComponent {
    public movie: ISearchMovieResult;
    private theMovidDbId: number;

    constructor(private searchService: SearchService, private route: ActivatedRoute,
                private sanitizer: DomSanitizer, private imageService: ImageService) {
        this.route.params.subscribe((params: any) => {
            this.theMovidDbId = params.movieDbId;
            this.load();
        });
    }

    public load() {
       this.searchService.getMovieInformation(this.theMovidDbId).subscribe(x => {
           this.movie = x;
           
            this.imageService.getMovieBanner(this.theMovidDbId.toString()).subscribe(x => {
                this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + x + ")");
            });
        });

    }
}
