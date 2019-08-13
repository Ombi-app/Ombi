import { Component, Input, ViewEncapsulation, OnInit, OnChanges, SimpleChanges } from "@angular/core";
import { IReleaseGroups } from "../../../../../interfaces/IMusicSearchResultV2";
import { SearchV2Service } from "../../../../../services/searchV2.service";
import { ActivatedRoute } from "@angular/router";

@Component({
    templateUrl: "./artist-release-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss", "./artist-release-panel.component.scss"],
    selector: "artist-release-panel",
    encapsulation: ViewEncapsulation.None
})
export class ArtistReleasePanel implements OnChanges {
    @Input() public releases: IReleaseGroups[];

    public albums: IReleaseGroups[];

    constructor(private searchService: SearchV2Service) {
    }

    ngOnChanges(changes: SimpleChanges): void {
        this.loadAlbums();
    }

    public ngOnInit() {
        this.loadAlbums();
    }

    private loadAlbums(): void {
        if (this.releases) {
            this.albums = this.releases.filter(x => x.releaseType === "Album");
            this.albums = this.albums.sort((a: IReleaseGroups, b: IReleaseGroups) =>  {
                return this.getTime(new Date(b.releaseDate)) - this.getTime(new Date(a.releaseDate));
            });
            
            this.albums.forEach(a => {
                this.searchService.getReleaseGroupArt(a.id).subscribe(x => a.image = x.image);
            });
        }
    }

    private getTime(date?: Date) {
        return date != null ? date.getTime() : 0;
    }
}
