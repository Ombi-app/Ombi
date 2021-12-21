import { Component, Input, ViewEncapsulation } from "@angular/core";
import { ISearchArtistResult } from "../../../../../interfaces";

@Component({
    templateUrl: "./album-information-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "album-information-panel",
    encapsulation: ViewEncapsulation.None
})
export class AlbumInformationPanel {
    @Input() public album: ISearchAlbumResult;
}
