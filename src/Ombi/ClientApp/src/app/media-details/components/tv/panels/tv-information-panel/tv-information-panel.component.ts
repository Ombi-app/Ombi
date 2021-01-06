import { Component, ViewEncapsulation, Input, OnInit } from "@angular/core";
import { ITvRequests } from "../../../../../interfaces";
import { ITvRatings } from "../../../../../interfaces/IRatings";
import { ISearchTvResultV2 } from "../../../../../interfaces/ISearchTvResultV2"; 
import { IStreamingData } from "../../../../../interfaces/IStreams";
import { SearchV2Service } from "../../../../../services";

@Component({
    templateUrl: "./tv-information-panel.component.html",
    styleUrls: ["../../../../media-details.component.scss"],
    selector: "tv-information-panel",
    encapsulation: ViewEncapsulation.None
})
export class TvInformationPanelComponent implements OnInit {

    constructor(private searchService: SearchV2Service) { }

    @Input() public tv: ISearchTvResultV2;
    @Input() public request: ITvRequests;
    @Input() public advancedOptions: boolean;

    public ratings: ITvRatings;
    public streams: IStreamingData[];
    public seasonCount: number;
    public totalEpisodes: number = 0;
    public nextEpisode: any;

    public ngOnInit(): void {
        this.searchService.getRottenTvRatings(this.tv.title, +this.tv.firstAired.toString().substring(0,4))
            .subscribe(x => this.ratings = x);

        this.searchService.getTvStreams(+this.tv.theTvDbId, this.tv.id).subscribe(x => this.streams = x);
        this.tv.seasonRequests.forEach(season => {
            this.totalEpisodes = this.totalEpisodes + season.episodes.length;
        });
        this.seasonCount = this.tv.seasonRequests.length;
    }
}
