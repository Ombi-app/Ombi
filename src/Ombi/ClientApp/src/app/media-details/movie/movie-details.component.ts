import { Component } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchMovieResultV2 } from "../../interfaces/ISearchMovieResultV2";
import { MatDialog } from "@angular/material";
import { YoutubeTrailerComponent } from "../youtube-trailer.component";
import { AuthService } from "../../auth/auth.service";

@Component({
    templateUrl: "./movie-details.component.html",
    styleUrls: ["../media-details.component.scss"],
})
export class MovieDetailsComponent {
    public movie: ISearchMovieResultV2;
    public isAdmin: boolean;
    private theMovidDbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService, private auth: AuthService) {
        this.route.params.subscribe((params: any) => {
            this.theMovidDbId = params.movieDbId;
            this.load();
        });
    }

    public load() {
        
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.searchService.getFullMovieDetails(this.theMovidDbId).subscribe(x => {
            this.movie = x;
            this.imageService.getMovieBanner(this.theMovidDbId.toString()).subscribe(x => {
                this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                    ("url(" + x + ")");
            });
        });

    }

    public async request() {
        var result = await this.requestService.requestMovie({ theMovieDbId: this.theMovidDbId, languageCode: null }).toPromise();
        if (result.result) {
            this.movie.requested = true;
            this.messageService.send(result.message, "Ok");
        } else {
            this.messageService.send(result.errorMessage, "Ok");
        }
    }

    public openDialog() {
        this.dialog.open(YoutubeTrailerComponent, {
            width: '560px',
            data: this.movie.videos.results[0].key
        });
    }
}
