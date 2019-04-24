import { Component, EventEmitter, Input, Output } from "@angular/core";

import { ISearchAlbumResult, ISearchArtistResult } from "../../interfaces/ISearchMusicResult";
import { SearchService } from "../../services";

@Component({
    selector: "artist-search",
    templateUrl: "./artistsearch.component.html",
})
export class ArtistSearchComponent {

    @Input() public result: ISearchArtistResult;
    @Input() public defaultPoster: string;
    public searchingAlbums: boolean;

    @Output() public viewAlbumsResult = new EventEmitter<ISearchAlbumResult[]>();

    constructor(private searchService: SearchService) {       
    }

    public viewAllAlbums() {
        this.searchingAlbums = true;
        this.searchService.getAlbumsForArtist(this.result.forignArtistId).subscribe(x => {
            this.viewAlbumsResult.emit(x);
        });
    }
}
