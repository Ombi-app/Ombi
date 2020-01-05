import { Component, AfterViewInit, ViewChild, EventEmitter, Output } from "@angular/core";
import { IMovieRequests, IRequestsViewModel } from "../../../interfaces";
import { MatPaginator, MatSort } from "@angular/material";
import { merge, Observable, of as observableOf } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

import { RequestServiceV2 } from "../../../services/requestV2.service";

@Component({
    templateUrl: "./movies-grid.component.html",
    selector: "movies-grid",
    styleUrls: ["../requests-list.component.scss"]
})
export class MoviesGridComponent implements AfterViewInit {
    public dataSource: IMovieRequests[] = [];
    public resultsLength: number;
    public isLoadingResults = true;
    public displayedColumns: string[] = ['requestedUser.requestedBy', 'title', 'requestedDate', 'status', 'requestStatus', 'actions'];
    public gridCount: string = "15";
    public showUnavailableRequests: boolean;
    
    @Output() public onOpenOptions = new EventEmitter<{request: any, filter: any}>();

    @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
    @ViewChild(MatSort, { static: false }) sort: MatSort;

    constructor(private requestService: RequestServiceV2) {

    }

    public async ngAfterViewInit() {
        // const results = await this.requestService.getMovieRequests(this.gridCount, 0, OrderType.RequestedDateDesc,
        //     { availabilityFilter: FilterType.None, statusFilter: FilterType.None }).toPromise();
        // this.dataSource = results.collection;
        // this.resultsLength = results.total;

        // If the user changes the sort order, reset back to the first page.
        this.sort.sortChange.subscribe(() => this.paginator.pageIndex = 0);

        merge(this.sort.sortChange, this.paginator.page)
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoadingResults = true;
                    // eturn this.exampleDatabase!.getRepoIssues(
                    //     this.sort.active, this.sort.direction, this.paginator.pageIndex);
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
            ).subscribe(data => this.dataSource = data);
    }

    public loadData(): Observable<IRequestsViewModel<IMovieRequests>> {
        if (this.showUnavailableRequests) {
            return this.requestService.getMovieUnavailableRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
        } else {
            return this.requestService.getMovieRequests(+this.gridCount, this.paginator.pageIndex * +this.gridCount, this.sort.active, this.sort.direction);
        }
    }

    public openOptions(request: IMovieRequests) {
        const filter = () => { this.dataSource = this.dataSource.filter((req) => {
            return req.id !== request.id;
        })};

        this.onOpenOptions.emit({request: request, filter: filter});
    }
}
