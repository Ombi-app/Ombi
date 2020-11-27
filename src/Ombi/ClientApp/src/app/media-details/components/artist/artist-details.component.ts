import { Component } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { AuthService } from "../../../auth/auth.service";
import { DenyDialogComponent } from "../shared/deny-dialog/deny-dialog.component";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { IArtistSearchResult, IReleaseGroups } from "../../../interfaces/IMusicSearchResultV2";

@Component({
    templateUrl: "./artist-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
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
        public messageService: MessageService, private auth: AuthService) {
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
                        this.messageService.send(r.message);
                    } else {
                        this.messageService.send(r.errorMessage);
                    }
                    
                    a.selected = false;
                })
                .catch(r => {
                    console.log(r);
                    this.messageService.send("Error when requesting album");
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
                        this.messageService.send(r.message);
                    } else {
                        this.messageService.send(r.errorMessage);
                    }
                    
                    a.selected = false;
                })
                .catch(r => {
                    console.log(r);
                    this.messageService.send("Error when requesting album");
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
