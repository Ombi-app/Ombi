import { Component, EventEmitter, Input, Output } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";

import { AuthService } from "../../auth/auth.service";
import { IRequestEngineResult, ISearchMovieResult } from "../../interfaces";
import { ISearchAlbumResult } from "../../interfaces/ISearchMusicResult";
import { NotificationService, RequestService } from "../../services";

@Component({
    selector: "album-search",
    templateUrl: "./albumsearch.component.html",
    styleUrls: ["./albumsearch.component.scss"],
})
export class AlbumSearchComponent {

    @Input() public result: ISearchAlbumResult;
    public engineResult: IRequestEngineResult;
    @Input() public defaultPoster: string;
    
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
                    this.engineResult = x;

                    if (this.engineResult.result) {
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
