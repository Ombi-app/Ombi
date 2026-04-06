import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { IAlbumRequest, IRequestsViewModel } from "../../../interfaces";
import { Observable, merge, of as observableOf } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { AuthService } from "../../../auth/auth.service";
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
import { GridSpinnerComponent } from "../grid-spinner/grid-spinner.component";

@Component({
    standalone: true,
    templateUrl: "./albums-grid.component.html",
    selector: "albums-grid",
    styleUrls: ["../_shared-card-grid.scss", "./albums-grid.component.scss"],
    imports: [
        CommonModule,
        RouterModule,
        MatPaginatorModule,
        MatButtonModule,
        MatFormFieldModule,
        MatSelectModule,
        TranslateModule,
        OmbiDatePipe,
        GridSpinnerComponent
    ]
})
export class AlbumsGridComponent implements OnInit, AfterViewInit {
    public dataSource: IAlbumRequest[] = [];
    public resultsLength: number;
    public isLoadingResults = true;
    public gridCount: string = "15";
    public isAdmin: boolean;
    public currentFilter: RequestFilterType = RequestFilterType.All;
    public manageOwnRequests: boolean;
    public userName: string;

    public RequestFilter = RequestFilterType;

    private sortActive: string = "requestedDate";
    private sortDirection: string = "desc";
    private storageKey = "Albums_DefaultRequestListSort";
    private storageKeyOrder = "Albums_DefaultRequestListSortOrder";
    private storageKeyGridCount = "Albums_DefaultGridCount";
    private storageKeyCurrentFilter = "Albums_DefaultFilter";

    @Output() public onOpenOptions = new EventEmitter<{ request: any, filter: any, onChange: any }>();

    @ViewChild(MatPaginator) paginator: MatPaginator;

    constructor(private requestService: RequestServiceV2, private ref: ChangeDetectorRef,
                private auth: AuthService, private storageService: StorageService) {

        this.userName = auth.claims().name;
    }

    public ngOnInit() {
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.manageOwnRequests = this.auth.hasRole("ManageOwnRequests")

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
                map((data: IRequestsViewModel<IAlbumRequest>) => {
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

    public loadData(): Observable<IRequestsViewModel<IAlbumRequest>> {
        switch(RequestFilterType[RequestFilterType[this.currentFilter]]) {
            case RequestFilterType.All:
                return this.requestService.getAlbumRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Pending:
                return this.requestService.getAlbumPendingRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Available:
                return this.requestService.getAlbumAvailableRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Processing:
                return this.requestService.getAlbumProcessingRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
            case RequestFilterType.Denied:
                return this.requestService.getAlbumDeniedRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sortActive, this.sortDirection);
        }
    }

    public openOptions(request: IAlbumRequest) {
        const filter = () => {
            this.dataSource = this.dataSource.filter((req) => {
                return req.id !== request.id;
            });
        };

        const onChange = () => {
            this.ref.detectChanges();
        };

        const data = { request: request, filter: filter, onChange: onChange, manageOwnRequests: this.manageOwnRequests, isAdmin: this.isAdmin, has4kRequest: false };
        this.onOpenOptions.emit(data);
    }

    public getStatusClass(item: IAlbumRequest): string {
        const status = (item as any).requestStatus?.toLowerCase() || '';
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
