import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Component, Input, OnInit, Inject } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged } from "rxjs/operators";
import { IIssueCategory, IRequestEngineResult } from "../../interfaces";
import { ISearchAlbumResult, ISearchArtistResult } from "../../interfaces/ISearchMusicResult";
import { SearchService } from "../../services";

@Component({
    selector: "music-search",
    templateUrl: "./musicsearch.component.html",
})
export class MusicSearchComponent implements OnInit {

    public searchText: string;
    public searchChanged: Subject<string> = new Subject<string>();
    public artistResult: ISearchArtistResult[];
    public albumResult: ISearchAlbumResult[];
    public result: IRequestEngineResult;
    public searchApplied = false;
    public searchAlbum: boolean = true;

    public musicRequested: Subject<void> = new Subject<void>();
    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    public issuesBarVisible = false;
    public issueRequestTitle: string;
    public issueRequestId: number;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;
    public defaultPoster: string;

    private href: string;
    constructor(
        private searchService: SearchService, private sanitizer: DomSanitizer,
         @Inject(APP_BASE_HREF) href:string) {
this.href = href;
        this.searchChanged.pipe(
            debounceTime(600), // Wait Xms after the last event before emitting last event
            distinctUntilChanged(), // only emit if value is different from previous value
        ).subscribe(x => {
            this.searchText = x as string;
            if (this.searchText === "") {
                if(this.searchAlbum) {
                    this.clearAlbumResults();
                } else {
                    this.clearArtistResults();
                }
                
                return;
            }
            if(this.searchAlbum) {
                if(!this.searchText) {
                    this.searchText = "iowa"; // REMOVE
                }
                this.searchService.searchAlbum(this.searchText)
                .subscribe(x => {
                    this.albumResult = x;
                    this.searchApplied = true;
                    this.setAlbumBackground();
                });
            } else {
                this.searchService.searchArtist(this.searchText)
                .subscribe(x => {
                    this.artistResult = x;
                    this.searchApplied = true;
                    this.setArtistBackground();
                });
            }
        });
        this.defaultPoster = "../../../images/default-music-placeholder.png";
        const base = this.href;
        if (base) {
            this.defaultPoster = "../../.." + base + "/images/default-music-placeholder.png";
        }
    }

    public ngOnInit() {
        this.searchText = "";
        this.artistResult = [];
        this.result = {
            message: "",
            result: false,
            errorMessage: "",
        };
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public searchMode(val: boolean) {
        this.searchAlbum = val;
        if(val) {
            // Album
            this.clearArtistResults();
        } else {
            this.clearAlbumResults();
        }
    }

    public setArtistSearch(artistId: string) {
        this.searchAlbum = false;
        this.clearAlbumResults();
        this.searchChanged.next(`lidarr:${artistId}`);
    }

    public viewAlbumsForArtist(albums: ISearchAlbumResult[]) {
        this.clearArtistResults();
        this.searchAlbum = true;
        this.albumResult = albums;
        this.setAlbumBackground();
    }

    private clearArtistResults() {
        this.artistResult = [];
        this.searchApplied = false;
    }

    private clearAlbumResults() {
        this.albumResult = [];
        this.searchApplied = false;
    }

    private setArtistBackground() {
        this.artistResult.forEach((val, index) => {
            if (val.poster === null) {
                val.poster = this.defaultPoster;
            } 
            val.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + val.banner + ")");
        });
    }

    private setAlbumBackground() {
        this.albumResult.forEach((val, index) => {
            if (val.disk === null) {
                if(val.cover === null) {
                    val.disk = this.defaultPoster;
                } else {
                    val.disk = val.cover;
                }
            }
            val.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + val.cover + ")");
        });
    }
}
