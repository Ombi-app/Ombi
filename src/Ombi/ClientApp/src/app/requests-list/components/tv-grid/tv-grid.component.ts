import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { IChildRequests, IRequestsViewModel } from "../../../interfaces";
import { Observable, merge, of as observableOf } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { AuthService } from "../../../auth/auth.service";
import { FeaturesFacade } from "../../../state/features/features.facade";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { RequestFilterType } from "../../models/RequestFilterType";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { StorageService } from "../../../shared/storage/storage-service";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { TranslateModule } from "@ngx-translate/core";
import { OmbiDatePipe } from "../../../pipes/OmbiDatePipe";
import { TranslateStatusPipe } from "../../../pipes/TranslateStatus";
import { GridSpinnerComponent } from "../grid-spinner/grid-spinner.component";

@Component({
    standalone: true,
    templateUrl: "./tv-grid.component.html",
    selector: "tv-grid",
    styleUrls: ["../_shared-card-grid.scss", "./tv-grid.component.scss"],
    imports: [
        CommonModule,
        RouterModule,
        MatPaginatorModule,
        MatButtonModule,
        MatFormFieldModule,
        MatSelectModule,
        TranslateModule,
        OmbiDatePipe,
        TranslateStatusPipe,
        GridSpinnerComponent
    ]
})
export class TvGridComponent implements OnInit, AfterViewInit {
    public dataSource: IChildRequests[] = [];
    public resultsLength: number;
    public isLoadingResults = true;
    public gridCount: string = "15";
    public isAdmin: boolean;
    public isPlayedSyncEnabled = false;
    public currentFilter: RequestFilterType = RequestFilterType.All;

    public RequestFilter = RequestFilterType;
    public manageOwnRequests: boolean;

    private sortActive: string = "requestedDate";
    private sortDirection: string = "desc";
    private storageKey = "Tv_DefaultRequestListSort";
    private storageKeyOrder = "Tv_DefaultRequestListSortOrder";
    private storageKeyGridCount = "Tv_DefaultGridCount";
    private storageKeyCurrentFilter = "Tv_DefaultFilter";

    @Output() public onOpenOptions = new EventEmitter<{ request: any, filter: any, onChange: any, manageOwnRequests: boolean, isAdmin: boolean, has4kRequest: boolean, hasRegularRequest: boolean }>();

    @ViewChild(MatPaginator) paginator: MatPaginator;

    constructor(private requestService: RequestServiceV2, private auth: AuthService,
                private ref: ChangeDetectorRef, private storageService: StorageService,
                private featureFacade: FeaturesFacade) {
    }

    public ngOnInit() {
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.isPlayedSyncEnabled = this.featureFacade.isPlayedSyncEnabled();

        const defaultCount = this.storageService.get(this.storageKeyGridCount);
        const defaultSort = this.storageService.get(this.storageKey);
        const defaultOrder = this.storageService.get(this.storageKeyOrder);
        const defaultFilter = +this.storageService.get(this.storageKeyCurrentFilter);
        if (defaultSort) {
            this.sortActive = defaultSort;
        }
        if (defaultOrder) {
            this.sortDirection = defaultOrder;
        }
        if (defaultCount) {
            this.gridCount = defaultCount;
        }
        if (defaultFilter) {
            this.currentFilter = defaultFilter;
        }
    }

    public async ngAfterViewInit() {
        this.storageService.save(this.storageKeyGridCount, this.gridCount);
        this.storageService.save(this.storageKeyCurrentFilter, (+this.currentFilter).toString());
        this.paginator.showFirstLastButtons = true;

        this.paginator.page
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoadingResults = true;
                    return this.loadData();
                }),
                map((data: IRequestsViewModel<IChildRequests>) => {
                    this.isLoadingResults = false;
                    this.resultsLength = data.total;
                    return data.collection;
                }),
                catchError(() => {
                    this.isLoadingResults = false;
                    return observableOf([]);
                })
            ).subscribe(data => this.dataSource = data);
    }

    public openOptions(request: IChildRequests) {
        const filter = () => { this.dataSource = this.dataSource.filter((req) => {
            return req.id !== request.id;
        })};

        const onChange = () => {
            this.ref.detectChanges();
        };

        const data = { request: request, filter: filter, onChange: onChange, manageOwnRequests: this.manageOwnRequests, isAdmin: this.isAdmin, has4kRequest: false, hasRegularRequest: true  };
        this.onOpenOptions.emit(data);
    }

    private loadData(): Observable<IRequestsViewModel<IChildRequests>> {
        switch(RequestFilterType[RequestFilterType[this.currentFilter]]) {
            case RequestFilterType.All:
                return this.requestService.getTvRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Pending:
                return this.requestService.getPendingTvRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Available:
                return this.requestService.getAvailableTvRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Processing:
                return this.requestService.getProcessingTvRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Denied:
                return this.requestService.getDeniedTvRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
        }
    }

    public getStatusClass(item: IChildRequests): string {
        const status = item.requestStatus?.toLowerCase() || '';
        if (status.includes('available')) return 'status-available';
        if (status.includes('pending') || status.includes('notyetrequest')) return 'status-pending';
        if (status.includes('processing') || status.includes('approved')) return 'status-processing';
        if (status.includes('denied')) return 'status-denied';
        return 'status-default';
    }

    public switchFilter(type: RequestFilterType) {
        this.currentFilter = type;
        this.ngAfterViewInit();
    }

    public onGridCountChange() {
        this.ngAfterViewInit();
    }
}
