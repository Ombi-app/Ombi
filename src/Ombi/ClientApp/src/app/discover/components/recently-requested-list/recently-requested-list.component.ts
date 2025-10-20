import { Component, OnInit, Input, ViewChild, OnDestroy, AfterViewInit, ElementRef, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatMenuModule } from "@angular/material/menu";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { TranslateModule } from "@ngx-translate/core";
import { CarouselModule, Carousel } from 'primeng/carousel';
import { SkeletonModule } from 'primeng/skeleton';

import { IRecentlyRequested, IRequestEngineResult, RequestType } from "../../../interfaces";
import { RequestServiceV2 } from "app/services/requestV2.service";
import { finalize, map, Observable, Subject, takeUntil, tap } from "rxjs";
import { Router } from "@angular/router";
import { AuthService } from "app/auth/auth.service";
import { NotificationService, RequestService } from "app/services";
import { TranslateService } from "@ngx-translate/core";
import { DenyDialogComponent } from '../../../media-details/components/shared/deny-dialog/deny-dialog.component';
import { MatDialog } from "@angular/material/dialog";
import { DetailedCardComponent } from "../../../components";

export enum DiscoverType {
    Upcoming,
    Trending,
    Popular,
    RecentlyRequested,
    Seasonal,
}

@Component({
    standalone: true,
    selector: "ombi-recently-list",
    templateUrl: "./recently-requested-list.component.html",
    styleUrls: ["./recently-requested-list.component.scss"],
    imports: [
        CommonModule,
        RouterModule,
        MatButtonModule,
        MatMenuModule,
        MatProgressSpinnerModule,
        TranslateModule,
        CarouselModule,
        SkeletonModule,
        DetailedCardComponent
    ]
})
export class RecentlyRequestedListComponent implements OnInit, OnDestroy, AfterViewInit {

    @Input() public id: string;
    @Input() public isAdmin: boolean;
    @ViewChild('carousel', {static: false}) carousel: Carousel;

    public requests$: Observable<IRecentlyRequested[]>;
    public requests = signal<IRecentlyRequested[]>([]);

    // Dynamic carousel sizing
    public numVisible = 3; // default starting value
    public numScroll = 1;  // default starting value
    private resizeObserver: ResizeObserver;
    public RequestType = RequestType;
    public loadingFlag: boolean;
    public DiscoverType = DiscoverType;

    private $loadSub = new Subject<void>();

    @ViewChild('carouselRoot', { static: false }) carouselRoot: ElementRef;

    constructor(private requestServiceV2: RequestServiceV2,
        private requestService: RequestService,
        private router: Router,
        private authService: AuthService,
        private notificationService: NotificationService,
        private translateService: TranslateService,
        public dialog: MatDialog) {
        // Disable default touch move override causing awkward scroll loops
        Carousel.prototype.onTouchMove = () => {};
    }

    public ngOnInit() {
        this.loadData();
        this.isAdmin = this.authService.isAdmin();
    }

    public ngAfterViewInit(): void {
        // Initialize observer after view is ready
        this.initResizeObserver();
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

    public deny(request: IRecentlyRequested) {
        const dialogRef = this.dialog.open(DenyDialogComponent, {
            width: '250px',
            data: { requestId: request.requestId, is4K: false, requestType: request.type }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
              this.notificationService.success(this.translateService.instant("Requests.SuccessfullyDenied"));
              request.denied = true;
          }
        });
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
        this.requestServiceV2.getRecentlyRequested().pipe(
            tap(() => this.loading()),
            takeUntil(this.$loadSub),
            finalize(() => this.finishLoading())
        ).subscribe(x => {
            this.requests.set(x || []);
            // Recalculate on data arrival to prevent blank space when fewer items than numVisible
            // Use setTimeout to allow the @defer block to render after signal update
            setTimeout(() => {
                // Initialize ResizeObserver now that the element should exist
                if (!this.resizeObserver) {
                    this.initResizeObserver();
                }
                this.recalculateCarouselDimensions();
            }, 0);
        });
    }

    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }

    // --- Dynamic sizing logic ---
    private initResizeObserver() {
        // Don't initialize observer if carouselRoot doesn't exist yet
        // It will be initialized after data loads
        if (!this.carouselRoot?.nativeElement) { return; }
        
        console.log('ResizeObserver: initializing');
        this.resizeObserver = new ResizeObserver(() => {
            this.recalculateCarouselDimensions();
        });
        this.resizeObserver.observe(this.carouselRoot.nativeElement);
    }

    private recalculateCarouselDimensions() {
        const containerEl: HTMLElement = this.carouselRoot?.nativeElement;
        if (!containerEl) { return; }
        // Prefer the actual visible content region for width calculations
        const itemsContent: HTMLElement | null = containerEl.querySelector('.p-carousel-items-content');
        const widthSource = itemsContent || containerEl;
        const width = widthSource.clientWidth;
        if (!width) { return; }

        // Try to measure an actual rendered item width for accurate calculation.
        let targetCardWidth: number;
        const firstItem: HTMLElement | null = containerEl.querySelector('.p-carousel-item ombi-detailed-card');
        if (firstItem) {
            const itemRect = firstItem.getBoundingClientRect();
            targetCardWidth = Math.ceil(itemRect.width) + 20; // Add padding
        }
        // Fallback heuristic if items not yet rendered
        if (!targetCardWidth || targetCardWidth === 0) {
            // Get viewport width for responsive heuristics
            const viewportWidth = (typeof globalThis.window !== 'undefined')
                ? (globalThis.window.innerWidth || globalThis.document.documentElement.clientWidth)
                : width; // fallback to component width if window unavailable
            targetCardWidth = viewportWidth <= 768 ? 220 : 420;
        }
        // Guard against pathological values
        if (targetCardWidth < 120) { targetCardWidth = 120; }

        const maxVisible = 8;
        let calculatedVisible = Math.max(1, Math.min(maxVisible, Math.floor(width / targetCardWidth)));
        const total = this.requests()?.length ?? 0;
        if (total && calculatedVisible > total) {
            calculatedVisible = total; // Avoid blank trailing space
        }
        this.numVisible = calculatedVisible || 1;

        const maxScroll = 3;
        // Adaptive scroll size: keep small, avoid overshooting the end
        this.numScroll = 1;
        if (total > this.numVisible) {
            const remainingAfterFirstPage = total - this.numVisible;
            if (remainingAfterFirstPage <= this.numVisible) {
                // Next scroll lands on last page precisely
                this.numScroll = Math.max(1, remainingAfterFirstPage);
            } else {
                // General case - scroll a portion, not full visible set
                this.numScroll = Math.min(maxScroll, this.numVisible);
            }
        }
    }

    // Ensure observer cleanup
    ngOnDestroy(): void {
        this.$loadSub.next();
        this.$loadSub.complete();
        this.resizeObserver?.disconnect();
    }
}
