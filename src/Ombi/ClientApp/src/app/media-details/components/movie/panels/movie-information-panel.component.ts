import { Component, ViewEncapsulation, Input } from "@angular/core";
import { ISearchMovieResultV2 } from "../../../../interfaces/ISearchMovieResultV2";
import { IAdvancedData, IMovieRequests } from "../../../../interfaces";

@Component({
    templateUrl: "./movie-information-panel.component.html",
    styleUrls: ["../../../media-details.component.scss"],
    selector: "movie-information-panel",
    encapsulation: ViewEncapsulation.None
})
export class MovieInformationPanelComponent {
    @Input() public movie: ISearchMovieResultV2;
    @Input() public request: IMovieRequests;
}
