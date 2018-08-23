import { PlatformLocation } from "@angular/common";
import { Component, Input, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { TranslateService } from "@ngx-translate/core";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged } from "rxjs/operators";

import { AuthService } from "../../auth/auth.service";
import { IIssueCategory, IRequestEngineResult, ISearchMovieResult } from "../../interfaces";
import { ISearchArtistResult } from "../../interfaces/ISearchMusicResult";
import { NotificationService, RequestService, SearchService } from "../../services";

@Component({
    selector: "music-search",
    templateUrl: "./musicsearch.component.html",
})
export class MusicSearchComponent implements OnInit {

    public searchText: string;
    public searchChanged: Subject<string> = new Subject<string>();
    public artistResult: ISearchArtistResult[];
    public result: IRequestEngineResult;
    public searchApplied = false;
    public searchAlbum: boolean;

    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    public issuesBarVisible = false;
    public issueRequestTitle: string;
    public issueRequestId: number;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;
    public defaultPoster: string;

    constructor(
        private searchService: SearchService, private requestService: RequestService,
        private notificationService: NotificationService, private authService: AuthService,
        private readonly translate: TranslateService,
        private readonly platformLocation: PlatformLocation,
        private sanitizer: DomSanitizer) {

        this.searchChanged.pipe(
            debounceTime(600), // Wait Xms after the last event before emitting last event
            distinctUntilChanged(), // only emit if value is different from previous value
        ).subscribe(x => {
            this.searchText = x as string;
            if (this.searchText === "") {
                this.clearResults();
                return;
            }
            if(this.searchAlbum) {
                this.searchService.searchAlbum(this.searchText)
                .subscribe(x => {
                    this.artistResult = x;
                    this.searchApplied = true;
                    this.setBackground();
                });
            } else {
                this.searchService.searchArtist(this.searchText)
                .subscribe(x => {
                    this.artistResult = x;
                    this.searchApplied = true;
                    this.setBackground();
                });
            }
        });
        this.defaultPoster = "../../../images/default_movie_poster.png";
        const base = this.platformLocation.getBaseHrefFromDOM();
        if (base) {
            this.defaultPoster = "../../.." + base + "/images/default_movie_poster.png";
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

    public request(searchResult: ISearchMovieResult) {
        searchResult.requested = true;
        searchResult.requestProcessing = true;
        searchResult.showSubscribe = false;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMovie")) {
            searchResult.approved = true;
        }

        try {
            this.requestService.requestMovie({ theMovieDbId: searchResult.id })
                .subscribe(x => {
                    this.result = x;

                    if (this.result.result) {
                        this.translate.get("Search.RequestAdded", { title: searchResult.title }).subscribe(x => {
                            this.notificationService.success(x);
                            searchResult.processed = true;
                        });
                    } else {
                        if (this.result.errorMessage && this.result.message) {
                            this.notificationService.warning("Request Added", `${this.result.message} - ${this.result.errorMessage}`);
                        } else {
                            this.notificationService.warning("Request Added", this.result.message ? this.result.message : this.result.errorMessage);
                        }
                        searchResult.requested = false;
                        searchResult.approved = false;
                        searchResult.processed = false;
                        searchResult.requestProcessing = false;

                    }
                });
        } catch (e) {

            searchResult.processed = false;
            searchResult.requestProcessing = false;
            this.notificationService.error(e);
        }
    }

    private clearResults() {
        this.artistResult = [];
        this.searchApplied = false;
    }

    private setBackground() {
        this.artistResult.forEach((val, index) => {
            if (val.poster === null) {
                val.poster = this.defaultPoster;
            }
            val.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + val.banner + ")");
        });
    }
}
