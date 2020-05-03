import { Component, AfterViewInit, ViewChild, Output, EventEmitter, ChangeDetectorRef, OnInit } from "@angular/core";
import {  IRequestsViewModel, IChildRequests } from "../../../interfaces";
import { MatPaginator, MatSort } from "@angular/material";
import { merge, of as observableOf, Observable } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { RequestServiceV2 } from "../../../services/requestV2.service";
import { AuthService } from "../../../auth/auth.service";
import { StorageService } from "../../../shared/storage/storage-service";

@Component({
    templateUrl: "./tv-grid.component.html",
    selector: "tv-grid",
    styleUrls: ["../requests-list.component.scss"]
})
export class TvGridComponent implements OnInit, AfterViewInit {
    public dataSource: IChildRequests[] = [];
    public resultsLength: number;
    public isLoadingResults = true;
    public displayedColumns: string[] = ['series',  'requestedBy', 'status', 'requestStatus', 'requestedDate','actions'];
    public gridCount: string = "15";
    public showUnavailableRequests: boolean;
    public isAdmin: boolean;
    public defaultSort: string = "requestedDate";
    public defaultOrder: string = "desc";

    private storageKey = "Tv_DefaultRequestListSort";
    private storageKeyOrder = "Tv_DefaultRequestListSortOrder";
    private storageKeyGridCount = "Tv_DefaultGridCount";

    @Output() public onOpenOptions = new EventEmitter<{request: any, filter: any, onChange: any}>();

    @ViewChild(MatPaginator, {static: false}) paginator: MatPaginator;
    @ViewChild(MatSort, {static: false}) sort: MatSort;

    constructor(private requestService: RequestServiceV2, private auth: AuthService,
                private ref: ChangeDetectorRef, private storageService: StorageService) {

    }

    public ngOnInit() {        
        const defaultCount = this.storageService.get(this.storageKeyGridCount);
        const defaultSort = this.storageService.get(this.storageKey);
        const defaultOrder = this.storageService.get(this.storageKeyOrder);
        if (defaultSort) {
            this.defaultSort = defaultSort;
        }
        if (defaultOrder) {
            this.defaultOrder = defaultOrder;
        }
        if (defaultCount) {
            this.gridCount = defaultCount;
        }
    }

    public async ngAfterViewInit() {

        this.storageService.save(this.storageKeyGridCount, this.gridCount);
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        // If the user changes the sort order, reset back to the first page.
        this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

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
                map((data: IRequestsViewModel<IChildRequests>) => {
                    // Flip flag to show that loading has finished.
                    this.isLoadingResults = false;
                    this.resultsLength = data.total;

                    return data.collection;
                }),
                catchError((err) => {
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

        this.onOpenOptions.emit({request: request, filter: filter, onChange});
    }

    private loadData(): Observable<IRequestsViewModel<IChildRequests>> {
        if(this.showUnavailableRequests) {
            return this.requestService.getTvUnavailableRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
        } else {
            return this.requestService.getTvRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
        }
    }
}
