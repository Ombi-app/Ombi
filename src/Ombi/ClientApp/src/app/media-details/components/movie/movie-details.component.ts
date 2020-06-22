import { Component, ViewEncapsulation } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { AuthService } from "../../../auth/auth.service";
import { IMovieRequests, RequestType, IAdvancedData } from "../../../interfaces";
import { DenyDialogComponent } from "../shared/deny-dialog/deny-dialog.component";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { StorageService } from "../../../shared/storage/storage-service";

@Component({
    templateUrl: "./movie-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class MovieDetailsComponent {
    public movie: ISearchMovieResultV2;
    public hasRequest: boolean;
    public movieRequest: IMovieRequests;
    public isAdmin: boolean;
    public advancedOptions: IAdvancedData;
    public showAdvanced: boolean; // Set on the UI

    private theMovidDbId: number;
    private imdbId: string;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService, private auth: AuthService,
        private storage: StorageService) {
        this.route.params.subscribe((params: any) => {
            if (typeof params.movieDbId === 'string' || params.movieDbId instanceof String) {
                if (params.movieDbId.startsWith("tt")) {
                    this.imdbId = params.movieDbId;
                }
            }
            this.theMovidDbId = params.movieDbId;
            this.load();
        });
    }

    public async load() {

        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");

        if (this.imdbId) {
            this.searchService.getMovieByImdbId(this.imdbId).subscribe(async x => {
                this.movie = x;
                if (this.movie.requestId > 0) {
                    // Load up this request
                    this.hasRequest = true;
                    this.movieRequest = await this.requestService.getMovieRequest(this.movie.requestId);
                }
                this.imageService.getMovieBanner(this.theMovidDbId.toString()).subscribe(x => {
                    this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                        ("url(" + x + ")");
                });
            });
        } else {
            this.searchService.getFullMovieDetails(this.theMovidDbId).subscribe(async x => {
                this.movie = x;
                if (this.movie.requestId > 0) {
                    // Load up this request
                    this.hasRequest = true;
                    this.movieRequest = await this.requestService.getMovieRequest(this.movie.requestId);
                }
                this.imageService.getMovieBanner(this.theMovidDbId.toString()).subscribe(x => {
                    this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                        ("url(" + x + ")");
                });
            });
        }
    }

    public async request() {
        const result = await this.requestService.requestMovie({ theMovieDbId: this.theMovidDbId, languageCode: null }).toPromise();
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

    public async deny() {
        const dialogRef = this.dialog.open(DenyDialogComponent, {
            width: '250px',
            data: { requestId: this.movieRequest.id, requestType: RequestType.movie }
        });

        dialogRef.afterClosed().subscribe(result => {
            this.movieRequest.denied = result;
            if (this.movieRequest.denied) {
                this.movie.approved = false;
            }
        });
    }

    public async issue() {
        const dialogRef = this.dialog.open(NewIssueComponent, {
            width: '500px',
            data: { requestId: this.movieRequest ? this.movieRequest.id : null, requestType: RequestType.movie, providerId: this.movie.imdbId ? this.movie.imdbId : this.movie.id, title: this.movie.title }
        });
    }

    public async approve() {
        const result = await this.requestService.approveMovie({ id: this.movieRequest.id }).toPromise();
        if (result.result) {
            this.movie.approved = false;
            this.messageService.send("Successfully Approved", "Ok");
        } else {
            this.messageService.send(result.errorMessage, "Ok");
        }
    }

    public async markAvailable() {
        const result = await this.requestService.markMovieAvailable({ id: this.movieRequest.id }).toPromise();
        if (result.result) {
            this.movie.available = true;
            this.messageService.send(result.message, "Ok");
        } else {
            this.messageService.send(result.errorMessage, "Ok");
        }
    }

    public setAdvancedOptions(data: IAdvancedData) {
        this.advancedOptions = data;
        if (data.rootFolderId) {
            this.movieRequest.qualityOverrideTitle = data.rootFolders.filter(x => x.id == data.rootFolderId)[0].path;
        }
        if (data.profileId) {
            this.movieRequest.rootPathOverrideTitle = data.profiles.filter(x => x.id == data.profileId)[0].name;
        }
    }
}
