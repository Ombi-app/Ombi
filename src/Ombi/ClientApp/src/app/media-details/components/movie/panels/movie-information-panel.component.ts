import { Component, Input, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ViewEncapsulation } from "@angular/core";
import { RouterModule } from "@angular/router";
import { TranslateModule } from "@ngx-translate/core";
import { MatChipsModule } from "@angular/material/chips";
import { MatTooltipModule } from "@angular/material/tooltip";
import { ISearchMovieResultV2 } from "../../../../interfaces/ISearchMovieResultV2";
import { IMovieRequests, RequestSource } from "../../../../interfaces";
import { SearchV2Service } from "../../../../services/searchV2.service";
import { IMovieRatings } from "../../../../interfaces/IRatings";
import { APP_BASE_HREF } from "@angular/common";
import { Inject } from "@angular/core";
import { IStreamingData } from "../../../../interfaces/IStreams";
import { TranslateStatusPipe } from "../../../../pipes/TranslateStatus";
import { OmbiDatePipe } from "../../../../pipes/OmbiDatePipe";
import { ThousandShortPipe } from "../../../../pipes/ThousandShortPipe";
import { QualityPipe } from "../../../../pipes/QualityPipe";

@Component({
    standalone: true,
    templateUrl: "./movie-information-panel.component.html",
    styleUrls: ["../../../media-details.component.scss"],
    selector: "movie-information-panel",
    encapsulation: ViewEncapsulation.None,
    imports: [
        CommonModule,
        RouterModule,
        TranslateModule,
        MatChipsModule,
        MatTooltipModule,
        TranslateStatusPipe,
        OmbiDatePipe,
        ThousandShortPipe,
        QualityPipe
    ]
})
export class MovieInformationPanelComponent implements OnInit {

    constructor(private searchService: SearchV2Service, @Inject(APP_BASE_HREF) public internalBaseUrl: string) { }

    @Input() public movie: ISearchMovieResultV2;
    @Input() public request: IMovieRequests;
    @Input() public advancedOptions: boolean;

    public ratings: IMovieRatings;
    public streams: IStreamingData[];
    public RequestSource = RequestSource;

    public baseUrl: string;

    public ngOnInit() {
        if (this.internalBaseUrl.length > 1) {
            this.baseUrl = this.internalBaseUrl;
        }
        // this.searchService.getRottenMovieRatings(this.movie.title, +this.movie.releaseDate.toString().substring(0,4))
        //     .subscribe(x => this.ratings = x);

            this.searchService.getMovieStreams(this.movie.id).subscribe(x => this.streams = x);
    }

    public getStatus(movie: ISearchMovieResultV2) {
      if (!movie.available && movie.requested) {
        if (movie.denied) {
          return "Common.RequestDenied";
        }
        if (movie.approved) {
          return "Common.ProcessingRequest";
        } else {
          return "Common.PendingApproval";
        }
      }

      if (!movie.available4K && movie.has4KRequest) {
        if (movie.denied4K) {
          return "Common.RequestDenied4K";
        }
        if (movie.approved4K) {
          return "Common.ProcessingRequest4K";
        } else {
          return "Common.PendingApproval4K";
        }
      }
    }
}
