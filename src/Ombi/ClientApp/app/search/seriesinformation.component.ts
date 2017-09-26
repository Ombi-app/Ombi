import { Component, Input, OnDestroy, OnInit} from "@angular/core";
//import { ActivatedRoute } from '@angular/router';
import { Subject } from "rxjs/Subject";

import "rxjs/add/operator/takeUntil";

import { NotificationService } from "../services";
import { RequestService } from "../services";
import { SearchService } from "../services";

import { IRequestEngineResult } from "../interfaces";
import { IEpisodesRequests } from "../interfaces";
import { ISearchTvResult } from "../interfaces";

@Component({
    selector: "seriesinformation",
    templateUrl: "./seriesinformation.component.html",
    styleUrls: ["./seriesinformation.component.scss"],
})
export class SeriesInformationComponent implements OnInit, OnDestroy {

    public result: IRequestEngineResult;
    public series: ISearchTvResult;
    public requestedEpisodes: IEpisodesRequests[] = [];

    @Input() private seriesId: number;
    private subscriptions = new Subject<void>();

    constructor(private searchService: SearchService, private requestService: RequestService, private notificationService: NotificationService) { }

    public ngOnInit() {
        this.searchService.getShowInformation(this.seriesId)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.series = x;
            });
    }

    public submitRequests() {
        this.series.requested = true;

        this.requestService.requestTv(this.series)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                debugger;
                this.result = x as IRequestEngineResult;
                if (this.result.requestAdded) {
                    this.notificationService.success("Request Added",
                        `Request for ${this.series.title} has been added successfully`);

                    this.series.seasonRequests.forEach((season) => {
                        season.episodes.forEach((ep) => {
                            ep.selected = false;
                        });
                    });

                } else {
                    this.notificationService.warning("Request Added", this.result.errorMessage ? this.result.errorMessage : this.result.message);
                }
            });
    }

    public addRequest(episode: IEpisodesRequests) {
        episode.requested = true;
        episode.selected = true;
    }

    public removeRequest(episode: IEpisodesRequests) {
        episode.requested = false;
        episode.selected = false;
    }

    public ngOnDestroy() {
        this.subscriptions.next();
        this.subscriptions.complete();
    }
}
