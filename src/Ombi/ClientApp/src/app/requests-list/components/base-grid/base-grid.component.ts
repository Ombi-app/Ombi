import { AfterViewInit, ChangeDetectorRef, Directive, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { IRequestsViewModel, IUserDropdown } from "../../../interfaces";
import { Observable, Subject, merge, of as observableOf } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { AuthService } from "../../../auth/auth.service";
import { IdentityService } from "../../../services/identity.service";
import { MatPaginator } from "@angular/material/paginator";
import { RequestFilterType } from "../../models/RequestFilterType";
import { StorageService } from "../../../shared/storage/storage-service";

export type RequestsViewMode = "cards" | "compact";

export interface GridSortOption {
    value: string;
    label: string;
}

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
    public viewMode: RequestsViewMode = "cards";
    public users: IUserDropdown[] = [];
    public selectedUserId: string = "";

    public readonly filterOptions = [
        { type: RequestFilterType.All, label: 'Requests.AllRequests', id: 'filterAll' },
        { type: RequestFilterType.Pending, label: 'Requests.PendingRequests', id: 'filterPending' },
        { type: RequestFilterType.Processing, label: 'Requests.ProcessingRequests', id: 'filterProcessing' },
        { type: RequestFilterType.Available, label: 'Requests.AvailableRequests', id: 'filterAvailable' },
        { type: RequestFilterType.Denied, label: 'Requests.DeniedRequests', id: 'filterDenied' },
    ];

    public sortOptions: GridSortOption[] = [
        { value: "requestedDate", label: "Requests.RequestDate" },
        { value: "title", label: "Requests.RequestsTitle" },
    ];

    public readonly gridCountOptions = [10, 15, 30, 100];

    public sortActive: string = "requestedDate";
    public sortDirection: string = "desc";
    private readonly reload$ = new Subject<void>();

    protected abstract storageKeySort: string;
    protected abstract storageKeySortOrder: string;
    protected abstract storageKeyGridCount: string;
    protected abstract storageKeyCurrentFilter: string;
    protected abstract storageKeyViewMode: string;

    @Output() public openOptionsEvent = new EventEmitter<any>();
    @ViewChild(MatPaginator) paginator: MatPaginator;

    constructor(
        protected auth: AuthService,
        protected ref: ChangeDetectorRef,
        protected storageService: StorageService,
        protected identityService: IdentityService
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
        const defaultViewMode = this.storageService.get(this.storageKeyViewMode);
        if (defaultSort) this.sortActive = defaultSort;
        if (defaultOrder) this.sortDirection = defaultOrder;
        if (defaultCount) this.gridCount = +defaultCount;
        if (defaultFilter) this.currentFilter = defaultFilter;
        if (defaultViewMode === "cards" || defaultViewMode === "compact") this.viewMode = defaultViewMode;

        if (this.isAdmin) {
            this.identityService.getUsersDropdown().subscribe(users => {
                this.users = users;
                this.ref.detectChanges();
            });
        }

        this.initFeatures();
    }

    protected initFeatures(): void {
        // Override in subclasses to initialize feature flags
    }

    public ngAfterViewInit() {
        this.paginator.showFirstLastButtons = true;

        merge(this.paginator.page, this.reload$)
            .pipe(
                startWith(null),
                switchMap(() => {
                    this.storageService.save(this.storageKeyGridCount, String(this.gridCount));
                    this.storageService.save(this.storageKeyCurrentFilter, (+this.currentFilter).toString());
                    this.storageService.save(this.storageKeySort, this.sortActive);
                    this.storageService.save(this.storageKeySortOrder, this.sortDirection);
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

    public switchView(mode: RequestsViewMode) {
        if (this.viewMode === mode) {
            return;
        }
        this.viewMode = mode;
        this.storageService.save(this.storageKeyViewMode, mode);
    }

    public get selectedUserName(): string {
        const user = this.users.find(x => x.id === this.selectedUserId);
        return user ? user.username : "";
    }

    public get activeSortLabel(): string {
        const option = this.sortOptions.find(x => x.value === this.sortActive);
        return option ? option.label : "";
    }

    public selectSort(value: string) {
        if (this.sortActive === value) {
            return;
        }
        this.sortActive = value;
        this.refresh(true);
    }

    public selectUserFilter(id: string) {
        if (this.selectedUserId === id) {
            return;
        }
        this.selectedUserId = id;
        this.refresh(true);
    }

    public selectGridCount(count: number) {
        if (this.gridCount === count) {
            return;
        }
        this.gridCount = count;
        this.refresh(true);
    }

    public onSortChange() {
        this.refresh(true);
    }

    public toggleSortDirection() {
        this.sortDirection = this.sortDirection === "asc" ? "desc" : "asc";
        this.refresh(true);
    }

    public setSort(field: string) {
        if (this.sortActive === field) {
            this.sortDirection = this.sortDirection === "asc" ? "desc" : "asc";
        } else {
            this.sortActive = field;
            this.sortDirection = "asc";
        }
        this.refresh(true);
    }

    public onUserFilterChange() {
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
        this.openOptionsEvent.emit({
            request, filter, onChange,
            manageOwnRequests: this.manageOwnRequests,
            isAdmin: this.isAdmin,
            ...extras
        });
    }
}
