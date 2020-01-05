import { Component, AfterViewInit, ViewChild, Output, EventEmitter } from "@angular/core";
import {  IRequestsViewModel, IChildRequests } from "../../../interfaces";
import { MatPaginator, MatSort } from "@angular/material";
import { merge, of as observableOf, Observable } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { RequestServiceV2 } from "../../../services/requestV2.service";

@Component({
    templateUrl: "./tv-grid.component.html",
    selector: "tv-grid",
    styleUrls: ["../requests-list.component.scss"]
})
export class TvGridComponent implements AfterViewInit {
    public dataSource: IChildRequests[] = [];
    public resultsLength: number;
    public isLoadingResults = true;
    public displayedColumns: string[] = ['series',  'requestedBy', 'status', 'requestStatus', 'requestedDate','actions'];
    public gridCount: string = "15";
    public showUnavailableRequests: boolean;

    @Output() public onOpenOptions = new EventEmitter<{request: any, filter: any}>();

    @ViewChild(MatPaginator, {static: false}) paginator: MatPaginator;
    @ViewChild(MatSort, {static: false}) sort: MatSort;

    constructor(private requestService: RequestServiceV2) {

    }

    public async ngAfterViewInit() {

        // If the user changes the sort order, reset back to the first page.
        this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

        merge(this.sort.sortChange, this.paginator.page)
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoadingResults = true;
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

        this.onOpenOptions.emit({request: request, filter: filter});
    }

    private loadData(): Observable<IRequestsViewModel<IChildRequests>> {
        if(this.showUnavailableRequests) {
            return this.requestService.getTvUnavailableRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
        } else {
            return this.requestService.getTvRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
        }
    }
}
