import { Component, ViewEncapsulation, Input } from "@angular/core";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";

@Component({
    templateUrl: "./tv-information-panel.component.html",
    styleUrls: ["../../media-details.component.scss"],
    selector: "tv-information-panel",
    encapsulation: ViewEncapsulation.None
})
export class TvInformationPanelComponent {
    @Input() public tv: ISearchTvResultV2;
}
