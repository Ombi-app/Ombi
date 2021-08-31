import { Component, ViewEncapsulation } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { AuthService } from "../../../auth/auth.service";
import { DenyDialogComponent } from "../shared/deny-dialog/deny-dialog.component";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { IAlbumSearchResult, IReleaseGroups } from "../../../interfaces/IMusicSearchResultV2";

@Component({
    templateUrl: "./album-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class AlbumDetailsComponent {
    private albumId: string;

    public album: IAlbumSearchResult = null;
    private selectedAlbums: IReleaseGroups[] = [];

    public isAdmin: boolean;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, private requestService: RequestService,
        public messageService: MessageService, private auth: AuthService) {
        this.route.params.subscribe((params: any) => {
            this.albumId = params.albumId;
            this.load();
        });
    }

    public load() {
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.searchService.getAlbumInformation(this.albumId).subscribe(x => this.album = x);
    }

    public getBackground(): string {
        if (this.album.cover) {
            this.album.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + this.album.cover + ")");
            return this.album.background
        }

        return this.album.background
    }

    public async requestAlbum() {
        if (this.album.monitored) {
            return;
        }
        this.requestService.requestAlbum({
            foreignAlbumId: this.album.id,
            monitored: true,
            monitor: "existing",
            searchForMissingAlbums: true
        }).toPromise()
        .then(r => {
            if (r.result) {
                this.album.monitored = true;
                this.messageService.send(r.message);
            } else {
                this.messageService.send(r.errorMessage);
            }
        })
        .catch(r => {
            console.log(r);
            this.messageService.send("Error when requesting album");
        });
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
