import { Component, OnInit, Input, ViewChild, Output, EventEmitter, OnDestroy } from "@angular/core";
import { DiscoverOption, IDiscoverCardResult } from "../../interfaces";
import { IRecentlyRequested, ISearchMovieResult, ISearchTvResult, RequestType } from "../../../interfaces";
import { SearchV2Service } from "../../../services";
import { StorageService } from "../../../shared/storage/storage-service";
import { MatButtonToggleChange } from '@angular/material/button-toggle';
import { Carousel } from 'primeng/carousel';
import { FeaturesFacade } from "../../../state/features/features.facade";
import { ResponsiveOptions } from "../carousel.options";
import { RequestServiceV2 } from "app/services/requestV2.service";
import { Subject, takeUntil } from "rxjs";
import { Router } from "@angular/router";

export enum DiscoverType {
    Upcoming,
    Trending,
    Popular,
    RecentlyRequested,
    Seasonal,
}

@Component({
    selector: "ombi-recently-list",
    templateUrl: "./recently-requested-list.component.html",
    styleUrls: ["./recently-requested-list.component.scss"],
})
export class RecentlyRequestedListComponent implements OnInit, OnDestroy {

    @Input() public id: string;
    @Input() public isAdmin: boolean;
    @ViewChild('carousel', {static: false}) carousel: Carousel;


    public requests: IRecentlyRequested[];

    public responsiveOptions: any;
    public RequestType = RequestType;
    public loadingFlag: boolean;
    public DiscoverType = DiscoverType;
    public is4kEnabled = false;

    private $loadSub = new Subject<void>();

    constructor(private requestService: RequestServiceV2,
        private featureFacade: FeaturesFacade,
        private router: Router) {
        Carousel.prototype.onTouchMove = () => {},
        this.responsiveOptions = ResponsiveOptions;
    }

    ngOnDestroy(): void {
        this.$loadSub.next();
        this.$loadSub.complete();
    }

    public ngOnInit() {
        this.loading();
        this.loadData();
    }

    public navigate(request: IRecentlyRequested) {
        this.router.navigate([this.generateDetailsLink(request), request.mediaId]);
    }

    private generateDetailsLink(request: IRecentlyRequested): string {
        switch (request.type) {
            case RequestType.movie:
                return `/details/movie/`;
            case RequestType.tvShow:
                return `/details/tv/`;
            case RequestType.album: //Actually artist
                return `/details/artist/`;
        }
    }

    private loadData() {
        this.requestService.getRecentlyRequested().pipe(takeUntil(this.$loadSub)).subscribe(x => {
            this.requests = x;
            this.finishLoading();
        });
    }


    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }
}
