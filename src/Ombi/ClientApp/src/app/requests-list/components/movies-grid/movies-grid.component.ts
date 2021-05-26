import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { IMovieRequests, IRequestEngineResult, IRequestsViewModel } from "../../../interfaces";
import { NotificationService, RequestService } from "../../../services";
import { Observable, forkJoin, merge, of as observableOf } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { AuthService } from "../../../auth/auth.service";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { MatTableDataSource } from "@angular/material/table";
import { RequestFilterType } from "../../models/RequestFilterType";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { SelectionModel } from "@angular/cdk/collections";
import { StorageService } from "../../../shared/storage/storage-service";
import { TranslateService } from "@ngx-translate/core";

@Component({
    templateUrl: "./movies-grid.component.html",
    selector: "movies-grid",
    styleUrls: ["./movies-grid.component.scss"]
})
export class MoviesGridComponent implements OnInit, AfterViewInit {
    public dataSource: MatTableDataSource<IMovieRequests>;
    public resultsLength: number;
    public isLoadingResults = true;
    public displayedColumns: string[] = ['title', 'requestedUser.requestedBy',  'status', 'requestStatus','requestedDate', 'actions'];
    public gridCount: string = "15";
    public isAdmin: boolean;
    public manageOwnRequests: boolean;
    public defaultSort: string = "requestedDate";
    public defaultOrder: string = "desc";
    public currentFilter: RequestFilterType = RequestFilterType.All;
    public selection = new SelectionModel<IMovieRequests>(true, []);

    public RequestFilter = RequestFilterType;


    private storageKey = "Movie_DefaultRequestListSort";
    private storageKeyOrder = "Movie_DefaultRequestListSortOrder";
    private storageKeyGridCount = "Movie_DefaultGridCount";
    private storageKeyCurrentFilter = "Movie_DefaultFilter";

    @Output() public onOpenOptions = new EventEmitter<{ request: any, filter: any, onChange: any, manageOwnRequests: boolean, isAdmin: boolean }>();

    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;

    constructor(private requestService: RequestServiceV2, private ref: ChangeDetectorRef,
                private auth: AuthService, private storageService: StorageService,
                private requestServiceV1: RequestService, private notification: NotificationService,
                private translateService: TranslateService) {

    }

    public ngOnInit() {
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.manageOwnRequests = this.auth.hasRole("ManageOwnRequests")
        if (this.isAdmin) {
            this.displayedColumns.unshift('select');
        }
        const defaultCount = this.storageService.get(this.storageKeyGridCount);
        const defaultSort = this.storageService.get(this.storageKey);
        const defaultOrder = this.storageService.get(this.storageKeyOrder);
        const defaultFilter = +this.storageService.get(this.storageKeyCurrentFilter);
        if (defaultSort) {
            this.defaultSort = defaultSort;
        }
        if (defaultOrder) {
            this.defaultOrder = defaultOrder;
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

        // If the user changes the sort order, reset back to the first page.
        this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);
        this.paginator.showFirstLastButtons = true;

        merge(this.sort.sortChange, this.paginator.page)
            .pipe(
                startWith({}),
                switchMap((value: any) => {
                    this.isLoadingResults = true;
                    if (value.active || value.direction) {
                        this.storageService.save(this.storageKey, value.active);
                        this.storageService.save(this.storageKeyOrder, value.direction);
                    }
                    return this.loadData();
                }),
                map((data: IRequestsViewModel<IMovieRequests>) => {
                    // Flip flag to show that loading has finished.
                    this.isLoadingResults = false;
                    this.resultsLength = data.total;

                    return data.collection;
                }),
                catchError((err) => {
                    this.isLoadingResults = false;
                    return observableOf([]);
                })
            ).subscribe(data => this.dataSource = new MatTableDataSource(data));
    }

    public loadData(): Observable<IRequestsViewModel<IMovieRequests>> {
        switch(RequestFilterType[RequestFilterType[this.currentFilter]]) {
            case RequestFilterType.All:
                return this.requestService.getMovieRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
            case RequestFilterType.Pending:
                return this.requestService.getMoviePendingRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
            case RequestFilterType.Available:
                return this.requestService.getMovieAvailableRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
            case RequestFilterType.Processing:
                return this.requestService.getMovieProcessingRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
            case RequestFilterType.Denied:
                return this.requestService.getMovieDeniedRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
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

        const data = { request: request, filter: filter, onChange: onChange, manageOwnRequests: this.manageOwnRequests, isAdmin: this.isAdmin };
        this.onOpenOptions.emit(data);
    }

    public switchFilter(type: RequestFilterType) {
        this.currentFilter = type;
        this.ngAfterViewInit();
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
        let tasks = new Array();
        this.selection.selected.forEach((selected) => {
            tasks.push(this.requestServiceV1.removeMovieRequestAsync(selected.id));
        });

        await Promise.all(tasks);

        this.notification.success(this.translateService.instant('Requests.RequestPanel.Deleted'))
        this.selection.clear();
        this.ngAfterViewInit();
      }

      public bulkApprove() {
        if (this.selection.isEmpty()) {
            return;
        }
        let tasks = new Array<Observable<IRequestEngineResult>>();
        this.selection.selected.forEach((selected) => {
            tasks.push(this.requestServiceV1.approveMovie({ id: selected.id }));
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
}