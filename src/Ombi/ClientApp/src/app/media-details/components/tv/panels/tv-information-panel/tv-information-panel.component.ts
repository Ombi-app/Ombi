import { Component, ViewEncapsulation, Input, OnInit } from "@angular/core";
import { ITvRequests } from "../../../../../interfaces";
import { ISearchTvResultV2 } from "../../../../../interfaces/ISearchTvResultV2"; 

@Component({
    templateUrl: "./tv-information-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "tv-information-panel",
    encapsulation: ViewEncapsulation.None
})
export class TvInformationPanelComponent implements OnInit {
    @Input() public tv: ISearchTvResultV2;
    @Input() public request: ITvRequests;
    @Input() public advancedOptions: boolean;

    public seasonCount: number;
    public totalEpisodes: number = 0;
    public nextEpisode: any;

    public ngOnInit(): void {
        this.tv.seasonRequests.forEach(season => {
            this.totalEpisodes = this.totalEpisodes + season.episodes.length;
        });
        this.seasonCount = this.tv.seasonRequests.length;
    }
}
