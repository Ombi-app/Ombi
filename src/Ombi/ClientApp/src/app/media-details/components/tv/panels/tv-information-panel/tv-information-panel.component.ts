import { APP_BASE_HREF } from "@angular/common";
import { Component, ViewEncapsulation, Input, OnInit, Inject } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
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

    constructor(private searchService: SearchV2Service, @Inject(APP_BASE_HREF) public internalBaseUrl: string, 
        private translate: TranslateService) { }

    @Input() public tv: ISearchTvResultV2;
    @Input() public request: ITvRequests;
    @Input() public advancedOptions: boolean;

    public ratings: ITvRatings;
    public streams: IStreamingData[];
    public seasonCount: number;
    public totalEpisodes: number = 0;
    public nextEpisode: any;
    public baseUrl: string;

    public ngOnInit(): void {
        if (this.internalBaseUrl.length > 1) {
            this.baseUrl = this.internalBaseUrl;
        }
        // this.searchService.getRottenTvRatings(this.tv.title, +this.tv.firstAired.toString().substring(0,4))
        //     .subscribe(x => this.ratings = x);

        this.searchService.getTvStreams(+this.tv.id ).subscribe(x => this.streams = x);
        this.tv.seasonRequests.forEach(season => {
            this.totalEpisodes = this.totalEpisodes + season.episodes.length;
        });
        this.seasonCount = this.tv.seasonRequests.length;
    }

    public sortBy(prop: string) {
        return this.streams.sort((a, b) => a[prop] > b[prop] ? 1 : a[prop] === b[prop] ? 0 : -1);
    }

    public getTVStatusLabel() {
        const textKey = 'MediaDetails.StatusValues.' + this.tv.status;
        const text = this.translate.instant(textKey);
        if (text !== textKey) {
            return text;
        } else {
            return this.tv.status;
        }
    }
}
