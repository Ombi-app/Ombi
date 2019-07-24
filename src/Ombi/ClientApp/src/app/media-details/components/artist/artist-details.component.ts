import { Component } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { MatDialog } from "@angular/material";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { AuthService } from "../../../auth/auth.service";
import { DenyDialogComponent } from "../shared/deny-dialog/deny-dialog.component";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { IArtistSearchResult } from "../../../interfaces/IMusicSearchResultV2";

@Component({
    templateUrl: "./artist-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
})
export class ArtistDetailsComponent {
    private artistId: string;

    public artist: IArtistSearchResult;

    public isAdmin: boolean;

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

    public async request() {
        // const result = await this.requestService.requestMovie({ theMovieDbId: this.theMovidDbId, languageCode: null }).toPromise();
        // if (result.result) {
        //     this.movie.requested = true;
        //     this.messageService.send(result.message, "Ok");
        // } else {
        //     this.messageService.send(result.errorMessage, "Ok");
        // }
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
