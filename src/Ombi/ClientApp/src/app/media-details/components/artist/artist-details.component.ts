import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatDialogModule } from "@angular/material/dialog";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatTabsModule } from "@angular/material/tabs";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { CarouselModule } from "primeng/carousel";
import { SkeletonModule } from "primeng/skeleton";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { AuthService } from "../../../auth/auth.service";
import { DenyDialogComponent } from "../shared/deny-dialog/deny-dialog.component";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { IArtistSearchResult, IReleaseGroups } from "../../../interfaces/IMusicSearchResultV2";
import { TranslateService } from "@ngx-translate/core";
import { TopBannerComponent } from "../shared/top-banner/top-banner.component";
import { SocialIconsComponent } from "../shared/social-icons/social-icons.component";
import { MediaPosterComponent } from "../shared/media-poster/media-poster.component";
import { ArtistInformationPanel } from "./panels/artist-information-panel/artist-information-panel.component";
import { ArtistReleasePanel } from "./panels/artist-release-panel/artist-release-panel.component";
import { MatCardModule } from "@angular/material/card";

@Component({
    standalone: true,
    templateUrl: "./artist-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatDialogModule,
        MatProgressSpinnerModule,
        MatTabsModule,
        MatTooltipModule,
        TranslateModule,
        CarouselModule,
        SkeletonModule,
        TopBannerComponent,
        SocialIconsComponent,
        MediaPosterComponent,
        ArtistInformationPanel,
        ArtistReleasePanel,
        MatCardModule
    ]
})
export class ArtistDetailsComponent {
    private artistId: string;

    public artist: IArtistSearchResult = null;

    public isAdmin: boolean;

    private selectedAlbums: IReleaseGroups[] = [];
    private allAlbums: IReleaseGroups[] = [];

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService, private auth: AuthService,
        private translate: TranslateService) {
        this.route.params.subscribe((params: any) => {
            this.artistId = params.artistId;
            this.load();
        });
    }

    public load() {
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.searchService.getArtistInformation(this.artistId).subscribe(x => this.artist = x);
    }

    public albumLoad(albums: IReleaseGroups[]) {
        albums.forEach(a => this.allAlbums.push(a));
    }

    public getBackground(): string {
        if (this.artist.fanArt) {
            this.artist.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + this.artist.fanArt + ")");
            return this.artist.background
        }
        if (this.artist.logo) {
            this.artist.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + this.artist.logo + ")");
            return this.artist.background
        }
        if (this.artist.poster) {
            this.artist.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + this.artist.poster + ")");
            return this.artist.background
        }

        return this.artist.background
    }

    public albumSelected(album: IReleaseGroups) {
        if (album.selected) {
            this.selectedAlbums.push(album);
        } else {
            const index = this.selectedAlbums.indexOf(album, 0);
            if (index > -1) {
                this.selectedAlbums.splice(index, 1);
            }
        }
    }

    
    public clearSelection() {
        this.selectedAlbums.forEach(a => a.selected = false);
        this.selectedAlbums = [];
    }

    public async requestAllAlbums() {
        
        if(this.selectedAlbums.length > 0) {
            this.selectedAlbums.forEach(async (a) => {
                if(a.monitored) {
                    return;
                }
                this.requestService.requestAlbum({
                    foreignAlbumId : a.id
                }).toPromise()
                .then(r => {
                    if (r.result) {
                        a.monitored = true;
                        this.messageService.send(this.translate.instant("Requests.RequestAddedSuccessfully", {title: a.title}));
                    } else {
                        this.messageService.sendRequestEngineResultError(r);
                    }
                    
                    a.selected = false;
                })
                .catch(r => {
                    console.log(r);
                    this.messageService.sendRequestEngineResultError(r);
                });
            });
        } else {
            this.allAlbums.forEach(async (a) => {
                if(a.monitored) {
                    return;
                }
                this.requestService.requestAlbum({
                    foreignAlbumId : a.id
                }).toPromise()
                .then(r => {
                    if (r.result) {
                        a.monitored = true;
                        this.messageService.send(this.translate.instant("Requests.RequestAddedSuccessfully", {title: a.title}));
                    } else {
                        this.messageService.sendRequestEngineResultError(r);
                    }
                    
                    a.selected = false;
                })
                .catch(r => {
                    console.log(r);
                    this.messageService.sendRequestEngineResultError(r);
                });
            })
        }

        this.selectedAlbums = [];
        // const 
    }

    public openDialog() {
        this.dialog.open(YoutubeTrailerComponent, {
            width: '560px',
            // data: this.movie.videos.results[0].key
        });
    }

    public async deny() {
        const dialogRef = this.dialog.open(DenyDialogComponent, {
            width: '250px',
            // data: {requestId: this.movieRequest.id,  requestType: RequestType.movie}
        });

        dialogRef.afterClosed().subscribe(result => {
            // this.movieRequest.denied = result;
            // if(this.movieRequest.denied) {
            //     this.movie.approved = false;
            // }
        });
    }

    public async issue() {
        const dialogRef = this.dialog.open(NewIssueComponent, {
            width: '500px',
            // data: {requestId: this.movieRequest ? this.movieRequest.id : null,  requestType: RequestType.movie, imdbid: this.movie.imdbId}
        });
    }

    public async approve() {
        // const result = await this.requestService.approveMovie({ id: this.movieRequest.id }).toPromise();
        // if (result.result) {
        //     this.movie.approved = false;
        //     this.messageService.send("Successfully Approved", "Ok");
        // } else {
        //     this.messageService.send(result.errorMessage, "Ok");
        // }
    }

    public async markAvailable() {
        // const result = await this.requestService.markMovieAvailable({id: this.movieRequest.id}).toPromise();
        // if (result.result) {
        //     // this.movie.available = true;
        //     this.messageService.send(result.message, "Ok");
        // } else {
        //     this.messageService.send(result.errorMessage, "Ok");
        // }
    }

    public setAdvancedOptions(data: any) {
        // this.advancedOptions = data;
    }
}
