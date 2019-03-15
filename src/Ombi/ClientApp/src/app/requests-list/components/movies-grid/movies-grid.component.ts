import { Component, OnInit } from "@angular/core";
import { RequestService } from "../../../services";
import { IMovieRequests, OrderType, FilterType, IRequestsViewModel } from "../../../interfaces";

@Component({
    templateUrl: "./movies-grid.component.html",
    selector: "movies-grid",
    styleUrls: ["../requests-list.component.scss"]
})
export class MoviesGridComponent implements OnInit {
    public dataSource: IRequestsViewModel<IMovieRequests>;

    public displayedColumns: string[] = ['requestedBy', 'title', 'requestedDate', 'status', 'requestStatus', 'actions'];

    constructor(private requestService: RequestService) {
        
    }

    public async ngOnInit() {
        this.dataSource = await this.requestService.getMovieRequests(30,0, OrderType.RequestedDateDesc,
             {availabilityFilter: FilterType.None, statusFilter: FilterType.None}).toPromise();
    }
}
