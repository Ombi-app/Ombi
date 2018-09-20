import { Component, EventEmitter, Input, Output } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";

import { Subject } from "rxjs";
import { AuthService } from "../../auth/auth.service";
import { IIssueCategory, IRequestEngineResult } from "../../interfaces";
import { ISearchAlbumResult } from "../../interfaces/ISearchMusicResult";
import { NotificationService, RequestService } from "../../services";

@Component({
    selector: "album-search",
    templateUrl: "./albumsearch.component.html",
})
export class AlbumSearchComponent {

    @Input() public result: ISearchAlbumResult;
    public engineResult: IRequestEngineResult;
    @Input() public defaultPoster: string;    

    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;
    
    @Input() public musicRequested: Subject<void>;
    public issuesBarVisible = false;
    public issueRequestTitle: string;
    public issueRequestId: number;
    public issueProviderId: string;
    public issueCategorySelected: IIssueCategory;
    
    @Output() public setSearch = new EventEmitter<string>();

    constructor(
        private requestService: RequestService,
        private notificationService: NotificationService, private authService: AuthService,
        private readonly translate: TranslateService) {       
    }

    public selectArtist(event: Event, artistId: string) {
        event.preventDefault();
        this.setSearch.emit(artistId);
    }

    public reportIssue(catId: IIssueCategory, req: ISearchAlbumResult) {
        this.issueRequestId = req.id;
        this.issueRequestTitle = req.title + `(${req.releaseDate.getFullYear})`;
        this.issueCategorySelected = catId;
        this.issuesBarVisible = true;
        this.issueProviderId = req.id.toString();
    }

    public request(searchResult: ISearchAlbumResult) {
        searchResult.requested = true;
        searchResult.requestProcessing = true;
        searchResult.showSubscribe = false;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMusic")) {
            searchResult.approved = true;
        }

        try {
            this.requestService.requestAlbum({ foreignAlbumId: searchResult.foreignAlbumId })
                .subscribe(x => {
                    
                    this.engineResult = x;

                    if (this.engineResult.result) {
                            this.musicRequested.next();
                            this.translate.get("Search.RequestAdded", { title: searchResult.title }).subscribe(x => {
                            this.notificationService.success(x);
                            searchResult.processed = true;
                        });
                    } else {
                        if (this.engineResult.errorMessage && this.engineResult.message) {
                            this.notificationService.warning("Request Added", `${this.engineResult.message} - ${this.engineResult.errorMessage}`);
                        } else {
                            this.notificationService.warning("Request Added", this.engineResult.message ? this.engineResult.message : this.engineResult.errorMessage);
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
}
