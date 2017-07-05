//import { Component, OnInit } from '@angular/core';
//import { DragulaService } from 'ng2-dragula/ng2-dragula';
//import { RequestService } from '../services/request.service';
//import { ITvRequests, IMovieRequests, IRequestGrid } from '../interfaces/IRequestModel';

//@Component({
//    templateUrl: './request-grid.component.html'
//})
//export class RequestGridComponent implements OnInit {

//    constructor(private dragulaService: DragulaService, private requestService: RequestService) {
//        this.dragulaService.drop.subscribe((value: any) => {
//        });
//    }


//    ngOnInit(): void {
//        this.requestService.getMovieGrid().subscribe(x => {
//            this.movieRequests = x;
//        });
//        this.requestService.getTvGrid().subscribe(x => {
//            this.tvRequests = x;
//        });
//    }


//    movieRequests: IRequestGrid<IMovieRequests>;
//    tvRequests: IRequestGrid<ITvRequests>;

//}