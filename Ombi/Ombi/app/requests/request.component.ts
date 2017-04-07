import { Component, OnInit } from '@angular/core';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { RequestService } from '../services/request.service';

import { IRequestModel } from '../interfaces/IRequestModel';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './request.component.html',
    providers: [RequestService]
})
export class RequestComponent implements OnInit {
    constructor(private requestService: RequestService) {  }

    requests: IRequestModel[];

    ngOnInit() {
        this.requestService.getAllRequests().subscribe(x => this.requests = x);
    }
}