import { Component, OnInit, Input } from "@angular/core";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType, ISearchTvResult, ISearchMovieResult, ISearchMovieResultContainer } from "../../../interfaces";
import { ImageService, RequestService, SearchV2Service } from "../../../services";
import { MatDialog } from "@angular/material/dialog";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";
import { EpisodeRequestComponent, EpisodeRequestData } from "../../../shared/episode-request/episode-request.component";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Router } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";

@Component({
    selector: "discover-grid",
    templateUrl: "./discover-grid.component.html",
    styleUrls: ["./discover-grid.component.scss"],
})
export class DiscoverGridComponent implements OnInit {

    @Input() public result: IDiscoverCardResult;
    public RequestType = RequestType;
    public requesting: boolean;

    public tv: ISearchTvResultV2;
    public tvCreator: string;
    public tvProducer: string;
    public movie: ISearchMovieResultV2;

    constructor(private searchService: SearchV2Service, private dialog: MatDialog,
        private requestService: RequestService, private notification: MatSnackBar,
        private router: Router, private sanitizer: DomSanitizer, private imageService: ImageService) { }

    public ngOnInit() {
        if (this.result.type == RequestType.tvShow) {
            this.getExtraTvInfo();
        }
        if (this.result.type == RequestType.movie) {
            this.getExtraMovieInfo();
        }
    }

    public async getExtraTvInfo() {
        this.tv = await this.searchService.getTvInfo(+this.result.id);
        this.setTvDefaults(this.tv);
        this.updateTvItem(this.tv);
        const creator = this.tv.crew.filter(tv => {
            return tv.type === "Creator";
        })[0];
        if (creator && creator.person) {
            this.tvCreator = creator.person.name;
        }
        const crewResult = this.tv.crew.filter(tv => {
            return tv.type === "Executive Producer";
        })[0]
        if (crewResult && crewResult.person) {
            this.tvProducer = crewResult.person.name;
        }
        this.setTvBackground();
    }

    public openDetails() {
        if (this.result.type === RequestType.movie) {
            this.router.navigate(['/details/movie/', this.result.id]);
        } else if (this.result.type === RequestType.tvShow) {
            this.router.navigate(['/details/tv/', this.result.id]);
        }
    }

    public getStatusClass(): string {
        if (this.result.available) {
            return "available";
        }
        if (this.result.approved) {
            return "approved";
        }
        if (this.result.requested) {
            return "requested";
        }
        return "notrequested";
    }

    private getExtraMovieInfo() {
        this.searchService.getFullMovieDetails(+this.result.id)
            .subscribe(m => {
                this.movie = m;
                this.updateMovieItem(m);
            });

        this.setMovieBackground()
    }

    private setMovieBackground(): void {
        this.result.background = this.sanitizer.bypassSecurityTrustStyle
            ("linear-gradient( rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5) ), url(" + "https://image.tmdb.org/t/p/original" + this.result.background + ")");
    }

    private setTvBackground(): void {
        if (this.result.background != null) {
            this.result.background = this.sanitizer.bypassSecurityTrustStyle
                ("linear-gradient( rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5) ), url(https://image.tmdb.org/t/p/original" + this.result.background + ")");
        } else {
            this.imageService.getTvBanner(+this.result.id).subscribe(x => {
                if (x) {
                    this.result.background = this.sanitizer.bypassSecurityTrustStyle
                        ("linear-gradient( rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5) ), url(" + x + ")");
                }
            });
        }
    }

    private updateMovieItem(updated: ISearchMovieResultV2) {
        this.result.url = "http://www.imdb.com/title/" + updated.imdbId + "/";
        this.result.available = updated.available;
        this.result.requested = updated.requested;
        this.result.requested = updated.requestProcessing;
        this.result.rating = updated.voteAverage;
    }


    private setTvDefaults(x: ISearchTvResultV2) {
        if (!x.imdbId) {
            x.imdbId = "https://www.tvmaze.com/shows/" + x.seriesId;
        } else {
            x.imdbId = "http://www.imdb.com/title/" + x.imdbId + "/";
        }
    }

    private updateTvItem(updated: ISearchTvResultV2) {
        this.result.title = updated.title;
        this.result.id = updated.id;
        this.result.available = updated.fullyAvailable;
        this.result.posterPath = updated.banner;
        this.result.requested = updated.requested;
        this.result.url = updated.imdbId;
    }

    public async request() {
        this.requesting = true;
        if (this.result.type === RequestType.movie) {
            const result = await this.requestService.requestMovie({ theMovieDbId: +this.result.id, languageCode: "", requestOnBehalf: null }).toPromise();

            if (result.result) {
                this.result.requested = true;
                this.notification.open(result.message, "Ok");
            } else {
                this.notification.open(result.errorMessage, "Ok");
            }
        } else if (this.result.type === RequestType.tvShow) {
            this.dialog.open(EpisodeRequestComponent, { width: "700px", data: <EpisodeRequestData>{ series: this.tv, requestOnBehalf: null }, panelClass: 'modal-panel' })
        }
        this.requesting = false;
    }

}
