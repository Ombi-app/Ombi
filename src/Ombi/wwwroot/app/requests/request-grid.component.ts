import { Component, OnInit } from '@angular/core';
import { DragulaService } from 'ng2-dragula/ng2-dragula';
import { RequestService } from '../services/request.service';
import { ITvRequestModel, IMovieRequestModel, IRequestGrid } from '../interfaces/IRequestModel';

@Component({
    moduleId: module.id,
    templateUrl: './request-grid.component.html'
})
export class RequestGridComponent implements OnInit {

    constructor(private dragulaService: DragulaService, private requestService: RequestService) {
        this.dragulaService.setOptions('requests', {
            removeOnSpill: false,

        });
    
        this.dragulaService.drop.subscribe((value: any) => {
            
        });
    }


    ngOnInit(): void {
        this.requestService.getMovieGrid().subscribe(x => {
            this.movieRequests = x;
        });
        this.requestService.getTvGrid().subscribe(x => {
            this.tvRequests = x;
        });
    }


    movieRequests: IRequestGrid<IMovieRequestModel>;
    tvRequests: IRequestGrid<ITvRequestModel>;

}