import { Component, Input, ViewEncapsulation, OnInit } from "@angular/core";
import { IReleaseGroups } from "../../../../../interfaces/IMusicSearchResultV2";

@Component({
    templateUrl: "./artist-release-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "artist-release-panel",
    encapsulation: ViewEncapsulation.None
})
export class ArtistReleasePanel implements OnInit {

    @Input() public releases: IReleaseGroups[];

    public albums: IReleaseGroups[];
    public singles: IReleaseGroups[];
    public ep: IReleaseGroups[];

    public ngOnInit(): void {
        this.albums = this.releases.filter(x => x.type === "Album");
        this.singles = this.releases.filter(x => x.type === "Single");
        this.ep = this.releases.filter(x => x.type === "EP");
    }
}
