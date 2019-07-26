import { Component, Input, ViewEncapsulation } from "@angular/core";
import { ISearchArtistResult } from "../../../../../interfaces";

@Component({
    templateUrl: "./artist-information-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "artist-information-panel",
    encapsulation: ViewEncapsulation.None
})
export class ArtistInformationPanel {
    @Input() public artist: ISearchArtistResult;
}
