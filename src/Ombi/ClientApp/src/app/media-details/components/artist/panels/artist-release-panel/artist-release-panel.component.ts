import { Component, Input, ViewEncapsulation, OnInit } from "@angular/core";
import { IReleaseGroups } from "../../../../../interfaces/IMusicSearchResultV2";
import { SearchV2Service } from "../../../../../services/searchV2.service";

@Component({
    templateUrl: "./artist-release-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "artist-release-panel",
    encapsulation: ViewEncapsulation.None
})
export class ArtistReleasePanel implements OnInit {

    @Input() public releases: IReleaseGroups[];

    public albums: IReleaseGroups[];

    constructor(private searchService: SearchV2Service) { }

    public ngOnInit() {
        this.albums = this.releases.filter(x => x.type === "Album");

        this.albums.forEach(a => {
            this.searchService.getReleaseGroupArt(a.id).subscribe(x => a.image = x.image);
        });
    }
}
