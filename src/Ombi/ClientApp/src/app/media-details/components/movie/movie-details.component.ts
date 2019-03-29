import { Component, ViewEncapsulation } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService, RadarrService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";
import { MatDialog } from "@angular/material";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { AuthService } from "../../../auth/auth.service";
import { IMovieRequests, IRadarrProfile, IRadarrRootFolder, RequestType } from "../../../interfaces";
import { DenyDialogComponent } from "../shared/deny-dialog/deny-dialog.component";

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

    public radarrProfiles: IRadarrProfile[];
    public radarrRootFolders: IRadarrRootFolder[];

    private theMovidDbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService, private auth: AuthService,
        private radarrService: RadarrService) {
        this.route.params.subscribe((params: any) => {
            this.theMovidDbId = params.movieDbId;
            this.load();
        });
    }

    public load() {

        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.searchService.getFullMovieDetails(this.theMovidDbId).subscribe(async x => {
            this.movie = x;
            if (this.movie.requestId > 0) {
                // Load up this request
                this.hasRequest = true;
                this.movieRequest = await this.requestService.getMovieRequest(this.movie.requestId);

                if (this.isAdmin) {
                    if (await this.radarrService.isRadarrEnabled()) {
                        this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
                            this.radarrProfiles = c;
                            this.setQualityOverrides();
                        });
                        this.radarrService.getRootFoldersFromSettings().subscribe(c => {
                            this.radarrRootFolders = c;
                            this.setRootFolderOverrides();
                        });
                    }
                }

            }
            this.imageService.getMovieBanner(this.theMovidDbId.toString()).subscribe(x => {
                this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                    ("url(" + x + ")");
            });
        });

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
            data: {requestId: this.movieRequest.id,  requestType: RequestType.movie}
          });
      
          dialogRef.afterClosed().subscribe(result => {
            this.movieRequest.denied = result;
            if(this.movieRequest.denied) {
                this.movie.approved = false;
            }
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
        const result = await this.requestService.markMovieAvailable({id: this.movieRequest.id}).toPromise();
        if (result.result) {
            this.movie.available = true;
            this.messageService.send(result.message, "Ok");
        } else {
            this.messageService.send(result.errorMessage, "Ok");
        }
    }

    private setQualityOverrides(): void {
        if (this.radarrProfiles) {
            const profile = this.radarrProfiles.filter((p) => {
                return p.id === this.movieRequest.qualityOverride;
            });
            if (profile.length > 0) {
                this.movieRequest.qualityOverrideTitle = profile[0].name;
            }
        }
    }

    private setRootFolderOverrides(): void {
        if (this.radarrRootFolders) {
            const path = this.radarrRootFolders.filter((folder) => {
                return folder.id === this.movieRequest.rootPathOverride;
            });
            if (path.length > 0) {
                this.movieRequest.rootPathOverrideTitle = path[0].path;
            }
        }
    }
}
