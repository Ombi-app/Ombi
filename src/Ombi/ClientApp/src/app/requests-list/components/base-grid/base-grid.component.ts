import { AfterViewInit, ChangeDetectorRef, Directive, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { IRequestsViewModel } from "../../../interfaces";
import { Observable, Subject, merge, of as observableOf } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { AuthService } from "../../../auth/auth.service";
import { MatPaginator } from "@angular/material/paginator";
import { RequestFilterType } from "../../models/RequestFilterType";
import { StorageService } from "../../../shared/storage/storage-service";

@Directive()
export abstract class BaseGridComponent<T> implements OnInit, AfterViewInit {
    public resultsLength: number;
    public isLoadingResults = true;
    public gridCount: number = 15;
    public isAdmin: boolean;
    public manageOwnRequests: boolean;
    public userName: string;
    public currentFilter: RequestFilterType = RequestFilterType.All;
    public RequestFilter = RequestFilterType;

    protected sortActive: string = "requestedDate";
    protected sortDirection: string = "desc";
    private readonly reload$ = new Subject<void>();

    protected abstract storageKeySort: string;
    protected abstract storageKeySortOrder: string;
    protected abstract storageKeyGridCount: string;
    protected abstract storageKeyCurrentFilter: string;

    @Output() public onOpenOptions = new EventEmitter<any>();
    @ViewChild(MatPaginator) paginator: MatPaginator;

    constructor(
        protected auth: AuthService,
        protected ref: ChangeDetectorRef,
        protected storageService: StorageService
    ) {
        this.userName = auth.claims().name;
    }

    public ngOnInit() {
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.manageOwnRequests = this.auth.hasRole("ManageOwnRequests");

        const defaultCount = this.storageService.get(this.storageKeyGridCount);
        const defaultSort = this.storageService.get(this.storageKeySort);
        const defaultOrder = this.storageService.get(this.storageKeySortOrder);
        const defaultFilter = +this.storageService.get(this.storageKeyCurrentFilter);
        if (defaultSort) this.sortActive = defaultSort;
        if (defaultOrder) this.sortDirection = defaultOrder;
        if (defaultCount) this.gridCount = +defaultCount;
        if (defaultFilter) this.currentFilter = defaultFilter;

        this.initFeatures();
    }

    protected initFeatures() {}

    public ngAfterViewInit() {
        this.paginator.showFirstLastButtons = true;

        merge(this.paginator.page, this.reload$)
            .pipe(
                startWith(null),
                switchMap(() => {
                    this.storageService.save(this.storageKeyGridCount, String(this.gridCount));
                    this.storageService.save(this.storageKeyCurrentFilter, (+this.currentFilter).toString());
                    this.isLoadingResults = true;
                    return this.loadData().pipe(
                        map((data: IRequestsViewModel<T>) => {
                            this.isLoadingResults = false;
                            this.resultsLength = data.total;
                            return data.collection;
                        }),
                        catchError(() => {
                            this.isLoadingResults = false;
                            this.resultsLength = 0;
                            return observableOf([]);
                        })
                    );
                }),
            ).subscribe(data => this.setData(data));
    }

    protected abstract setData(data: T[]): void;

    protected abstract loadData(): Observable<IRequestsViewModel<T>>;

    public getStatusClass(item: any): string {
        const status = (item.requestStatus || '').toLowerCase();
        if (status.includes('available')) return 'status-available';
        if (status.includes('pending') || status.includes('notyetrequest')) return 'status-pending';
        if (status.includes('processing') || status.includes('approved')) return 'status-processing';
        if (status.includes('denied')) return 'status-denied';
        return 'status-default';
    }

    public switchFilter(type: RequestFilterType) {
        this.currentFilter = type;
        this.refresh(true);
    }

    public onGridCountChange() {
        this.refresh(true);
    }

    protected refresh(resetPage = false) {
        if (resetPage) {
            this.paginator.firstPage();
        }
        this.reload$.next();
    }

    protected abstract removeFromDataSource(id: number): void;

    protected emitOptions(request: any, extras: Record<string, any> = {}) {
        const filter = () => {
            this.removeFromDataSource(request.id);
            this.resultsLength = Math.max(0, this.resultsLength - 1);
        };
        const onChange = () => this.ref.detectChanges();
        this.onOpenOptions.emit({
            request, filter, onChange,
            manageOwnRequests: this.manageOwnRequests,
            isAdmin: this.isAdmin,
            ...extras
        });
    }
}
