import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ISearchArtistResult } from "../../../../../interfaces";
import { ViewEncapsulation } from "@angular/core";

@Component({
        standalone: true,
    templateUrl: "./artist-information-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "artist-information-panel",
    encapsulation: ViewEncapsulation.None,
    imports: [
        CommonModule
    ]
})
export class ArtistInformationPanel {
    @Input() public artist: ISearchArtistResult;
}
