import { Component, AfterViewInit, ViewChild } from "@angular/core";
import { RequestService } from "../../../services";
import { IMovieRequests, OrderType, FilterType, IRequestsViewModel } from "../../../interfaces";
import { MatPaginator, MatSort } from "@angular/material";
import { merge, Observable, of as observableOf } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';

@Component({
    templateUrl: "./movies-grid.component.html",
    selector: "movies-grid",
    styleUrls: ["../requests-list.component.scss"]
})
export class MoviesGridComponent implements AfterViewInit {
    public dataSource: IMovieRequests[] = [];
    public resultsLength: number;
    public isLoadingResults = true;
    public displayedColumns: string[] = ['requestedBy', 'title', 'requestedDate', 'status', 'requestStatus', 'actions'];
    public gridCount = 30;

    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;

    constructor(private requestService: RequestService) {

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
                    return this.requestService.getMovieRequests(this.gridCount, this.paginator.pageIndex * this.gridCount, OrderType.RequestedDateDesc, 
                        { availabilityFilter: FilterType.None, statusFilter: FilterType.None});
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
}
