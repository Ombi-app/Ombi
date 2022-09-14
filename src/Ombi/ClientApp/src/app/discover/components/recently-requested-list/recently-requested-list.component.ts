import { Component, OnInit, Input, ViewChild, OnDestroy } from "@angular/core";
import { IRecentlyRequested, IRequestEngineResult, RequestType } from "../../../interfaces";
import { Carousel } from 'primeng/carousel';
import { ResponsiveOptions } from "../carousel.options";
import { RequestServiceV2 } from "app/services/requestV2.service";
import { finalize, map, Observable, Subject, takeUntil, tap } from "rxjs";
import { Router } from "@angular/router";
import { AuthService } from "app/auth/auth.service";
import { NotificationService, RequestService } from "app/services";
import { TranslateService } from "@ngx-translate/core";

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

    public requests$: Observable<IRecentlyRequested[]>;

    public responsiveOptions: any;
    public RequestType = RequestType;
    public loadingFlag: boolean;
    public DiscoverType = DiscoverType;

    private $loadSub = new Subject<void>();

    constructor(private requestServiceV2: RequestServiceV2,
        private requestService: RequestService,
        private router: Router,
        private authService: AuthService,
        private notificationService: NotificationService,
        private translateService: TranslateService) {
        Carousel.prototype.onTouchMove = () => {};
        this.responsiveOptions = ResponsiveOptions;
    }

    ngOnDestroy(): void {
        this.$loadSub.next();
        this.$loadSub.complete();
    }

    public ngOnInit() {
        this.loadData();
        this.isAdmin = this.authService.isAdmin();
    }

    public navigate(request: IRecentlyRequested) {
        this.router.navigate([this.generateDetailsLink(request), request.mediaId]);
    }

    public approve(request: IRecentlyRequested) {
        switch(request.type) {
            case RequestType.movie:
                this.requestService.approveMovie({id: request.requestId, is4K: false}).pipe(
                    map((res) => this.handleApproval(res, request))
                ).subscribe();
                break;
            case RequestType.tvShow:
                this.requestService.approveChild({id: request.requestId}).pipe(
                    tap((res) => this.handleApproval(res, request))
                ).subscribe();
                break;
            case RequestType.album:
                this.requestService.approveAlbum({id: request.requestId}).pipe(
                    tap((res) => this.handleApproval(res, request))
                ).subscribe();
                break;
        }
    }

    private handleApproval(result: IRequestEngineResult, request: IRecentlyRequested) {
        if (result.result) {
            this.notificationService.success(this.translateService.instant("Requests.SuccessfullyApproved"));
            request.approved = true;
        } else {
            this.notificationService.error(result.errorMessage);
        }
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
        this.requests$ = this.requestServiceV2.getRecentlyRequested().pipe(
            tap(() => this.loading()),
            takeUntil(this.$loadSub),
            finalize(() => this.finishLoading())
        );
    }

    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }
}
