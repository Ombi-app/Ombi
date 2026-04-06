import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { IMovieRequests, IRequestEngineResult, IRequestsViewModel } from "../../../interfaces";
import { NotificationService, RequestService } from "../../../services";
import { Observable, combineLatest, forkJoin, merge, of as observableOf, Subject } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { AuthService } from "../../../auth/auth.service";
import { FeaturesFacade } from "../../../state/features/features.facade";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { MatTableDataSource } from "@angular/material/table";
import { RequestFilterType } from "../../models/RequestFilterType";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { SelectionModel } from "@angular/cdk/collections";
import { StorageService } from "../../../shared/storage/storage-service";
import { TranslateService } from "@ngx-translate/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { GridSpinnerComponent } from "../grid-spinner/grid-spinner.component";
import { TranslateModule } from "@ngx-translate/core";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatOptionModule } from "@angular/material/core";
import { MatSelectModule } from "@angular/material/select";
import { MatMenuModule } from "@angular/material/menu";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { OmbiDatePipe } from "../../../pipes/OmbiDatePipe";
import { TranslateStatusPipe } from "../../../pipes/TranslateStatus";

@Component({
    standalone: true,
    templateUrl: "./movies-grid.component.html",
    selector: "movies-grid",
    styleUrls: ["../_shared-card-grid.scss", "./movies-grid.component.scss"],
    imports: [
        CommonModule,
        RouterModule,
        GridSpinnerComponent,
        TranslateModule,
        MatFormFieldModule,
        MatOptionModule,
        MatPaginatorModule,
        MatSelectModule,
        MatMenuModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        MatCheckboxModule,
        OmbiDatePipe,
        TranslateStatusPipe
    ]
})
export class MoviesGridComponent implements OnInit, AfterViewInit {
    public dataSource: MatTableDataSource<IMovieRequests>;
    public resultsLength: number;
    public isLoadingResults = true;
    public gridCount: string = "15";
    public isAdmin: boolean;
    public is4kEnabled = false;
    public isPlayedSyncEnabled = false;
    public manageOwnRequests: boolean;
    public currentFilter: RequestFilterType = RequestFilterType.All;
    public selection = new SelectionModel<IMovieRequests>(true, []);
    public userName: string;

    public RequestFilter = RequestFilterType;

    private sortActive: string = "requestedDate";
    private sortDirection: string = "desc";
    private storageKey = "Movie_DefaultRequestListSort";
    private storageKeyOrder = "Movie_DefaultRequestListSortOrder";
    private storageKeyGridCount = "Movie_DefaultGridCount";
    private storageKeyCurrentFilter = "Movie_DefaultFilter";

    @Output() public onOpenOptions = new EventEmitter<{ request: any, filter: any, onChange: any, manageOwnRequests: boolean, isAdmin: boolean, has4kRequest: boolean, hasRegularRequest: boolean }>();

    @ViewChild(MatPaginator) paginator: MatPaginator;

    constructor(private requestService: RequestServiceV2, private ref: ChangeDetectorRef,
        private auth: AuthService, private storageService: StorageService,
        private requestServiceV1: RequestService, private notification: NotificationService,
        private translateService: TranslateService,
        private featureFacade: FeaturesFacade) {

        this.userName = auth.claims().name;
    }

    public ngOnInit() {
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.manageOwnRequests = this.auth.hasRole("ManageOwnRequests")

        this.is4kEnabled = this.featureFacade.is4kEnabled();
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
                map((data: IRequestsViewModel<IMovieRequests>) => {
                    this.isLoadingResults = false;
                    this.resultsLength = data.total;
                    return data.collection;
                }),
                catchError(() => {
                    this.isLoadingResults = false;
                    return observableOf([]);
                })
            ).subscribe(data => this.dataSource = new MatTableDataSource(data));
    }

    public loadData(): Observable<IRequestsViewModel<IMovieRequests>> {
        switch(RequestFilterType[RequestFilterType[this.currentFilter]]) {
            case RequestFilterType.All:
                return this.requestService.getMovieRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Pending:
                return this.requestService.getMoviePendingRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Available:
                return this.requestService.getMovieAvailableRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Processing:
                return this.requestService.getMovieProcessingRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Denied:
                return this.requestService.getMovieDeniedRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
        }
    }

    public openOptions(request: IMovieRequests) {
        const filter = () => {
            this.dataSource.data = this.dataSource.data.filter((req) => {
                return req.id !== request.id;
            });
        };

        const onChange = () => {
            this.ref.detectChanges();
        };

        const data = { request: request, filter: filter, onChange: onChange, manageOwnRequests: this.manageOwnRequests, isAdmin: this.isAdmin, has4kRequest: request.has4KRequest, hasRegularRequest: this.checkDate(request.requestedDate) };
        this.onOpenOptions.emit(data);
    }

    private checkDate(date: Date|string): boolean {
        if (typeof date === 'string') {
            return new Date(date).getFullYear() > 1;
        }
        if (date instanceof Date) {
            return date.getFullYear() > 1;
        }
        return false;
    }

    public switchFilter(type: RequestFilterType) {
        this.currentFilter = type;
        this.ngAfterViewInit();
    }

    public onGridCountChange() {
        this.ngAfterViewInit();
    }

    public getStatusClass(item: IMovieRequests): string {
        const status = item.requestStatus?.toLowerCase() || '';
        if (status.includes('available')) return 'status-available';
        if (status.includes('pending') || status.includes('notyetrequest')) return 'status-pending';
        if (status.includes('processing') || status.includes('approved')) return 'status-processing';
        if (status.includes('denied')) return 'status-denied';
        return 'status-default';
    }

    public isAllSelected() {
        const numSelected = this.selection.selected.length;
        const numRows = this.dataSource.data.length;
        return numSelected === numRows;
    }

    public masterToggle() {
        this.isAllSelected() ?
            this.selection.clear() :
            this.dataSource.data.forEach(row => this.selection.select(row));
    }

    public async bulkDelete() {
        if (this.selection.isEmpty()) {
            return;
        }
        let tasks = new Array<Observable<IRequestEngineResult>>();
        this.selection.selected.forEach((selected) => {
            tasks.push(this.requestServiceV1.removeMovieRequestAsync(selected.id));
        });

        combineLatest(tasks).subscribe(() => {
            this.notification.success(this.translateService.instant('Requests.RequestPanel.Deleted'))
            this.selection.clear();
            this.ngAfterViewInit();
        });
    }

    public bulkApprove = () => this.bulkApproveInternal(false);

    public bulkApprove4K = () => this.bulkApproveInternal(true);

    private bulkApproveInternal(is4k: boolean) {
        if (this.selection.isEmpty()) {
            return;
        }
        let tasks = new Array<Observable<IRequestEngineResult>>();
        this.selection.selected.forEach((selected) => {
            tasks.push(this.requestServiceV1.approveMovie({ id: selected.id, is4K: is4k }));
        });

        this.isLoadingResults = true;
        forkJoin(tasks).subscribe((result: IRequestEngineResult[]) => {
            this.isLoadingResults = false;
            const failed = result.filter(x => !x.result);
            if(failed.length > 0) {
                this.notification.error("Some requests failed to approve: " + failed[0].errorMessage);
                this.selection.clear();
                return;
            }
            this.notification.success(this.translateService.instant('Requests.RequestPanel.Approved'));
            this.selection.clear();
            this.ngAfterViewInit();
        })
    }

    public bulkDeny = () => this.bulkDenyInternal(false);

    public bulkDeny4K = () => this.bulkDenyInternal(true);

    private bulkDenyInternal(is4k: boolean) {
        if (this.selection.isEmpty()) {
            return;
        }
        let tasks = new Array<Observable<IRequestEngineResult>>();
        this.selection.selected.forEach((selected) => {

            tasks.push(this.requestServiceV1.denyMovie({
                id: selected.id,
                is4K: is4k,
                reason: ``
            }));
        });

        this.isLoadingResults = true;
        forkJoin(tasks).subscribe((result: IRequestEngineResult[]) => {
            this.isLoadingResults = false;
            const failed = result.filter(x => !x.result);
            if (failed.length > 0) {
                this.notification.error("Some requests failed to deny: " + failed[0].errorMessage);
                this.selection.clear();
                return;
            }
            this.notification.success(this.translateService.instant('Requests.RequestPanel.Denied'));
            this.selection.clear();
            this.ngAfterViewInit();
        })
    }

    public getRequestDate(request: IMovieRequests): Date {
        if (new Date(request.requestedDate).getFullYear() === 1) {
            return request.requestedDate4k;
        }
        return request.requestedDate;
    }
}
