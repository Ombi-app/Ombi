import { Component, ViewEncapsulation, Input, OnInit, Inject } from "@angular/core";
import { ISearchMovieResultV2 } from "../../../../interfaces/ISearchMovieResultV2";
import { IMovieRequests } from "../../../../interfaces";
import { SearchV2Service } from "../../../../services/searchV2.service";
import { IMovieRatings } from "../../../../interfaces/IRatings";
import { APP_BASE_HREF } from "@angular/common";
import { IStreamingData } from "../../../../interfaces/IStreams";
@Component({
    templateUrl: "./movie-information-panel.component.html",
    styleUrls: ["../../../media-details.component.scss"],
    selector: "movie-information-panel",
    encapsulation: ViewEncapsulation.None
})
export class MovieInformationPanelComponent implements OnInit {

    constructor(private searchService: SearchV2Service, @Inject(APP_BASE_HREF) public baseUrl: string) { }

    @Input() public movie: ISearchMovieResultV2;
    @Input() public request: IMovieRequests;
    @Input() public advancedOptions: boolean;

    public ratings: IMovieRatings;
    public streams: IStreamingData[];

    public ngOnInit() {
        this.searchService.getRottenMovieRatings(this.movie.title, +this.movie.releaseDate.toString().substring(0,4))
            .subscribe(x => this.ratings = x);

            this.searchService.getMovieStreams(this.movie.id).subscribe(x => this.streams = x);
    }
}
