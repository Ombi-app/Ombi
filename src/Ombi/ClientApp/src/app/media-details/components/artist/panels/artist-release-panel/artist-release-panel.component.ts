import { Component, Input, ViewEncapsulation, OnInit } from "@angular/core";
import { IReleaseGroups } from "../../../../../interfaces/IMusicSearchResultV2";
import { SearchV2Service } from "../../../../../services/searchV2.service";
import { ActivatedRoute } from "@angular/router";

@Component({
    templateUrl: "./artist-release-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "artist-release-panel",
    encapsulation: ViewEncapsulation.None
})
export class ArtistReleasePanel implements OnInit {

    @Input() public releases: IReleaseGroups[];

    public albums: IReleaseGroups[];

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute) {
        route.params.subscribe(() => {
            // This is due to when we change the music Id, NgOnInit is not called again
            // Since the component has not been destroyed (We are not navigating away)
            // so we need to subscribe to custom changes so we can do the data manulipulation below
            this.loadAlbums();
        });
    }

    public ngOnInit() {
        this.loadAlbums();
    }

    private loadAlbums(): void {
        this.albums = this.releases.filter(x => x.type === "Album");

        this.albums.forEach(a => {
            this.searchService.getReleaseGroupArt(a.id).subscribe(x => a.image = x.image);
        });
    }
}
