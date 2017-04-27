import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { SearchService } from '../services/search.service';
//import { RequestService } from '../services/request.service';
//import { NotificationService } from '../services/notification.service';

import { ISearchTvResult } from '../interfaces/ISearchTvResult';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';

@Component({
    selector: 'tv-search',
    moduleId: module.id,
    templateUrl: './tvsearch.component.html',
})
export class TvSearchComponent implements OnInit {

    searchText: string;
    searchChanged: Subject<string> = new Subject<string>();
    tvResults: ISearchTvResult[];
    result: IRequestEngineResult;

    constructor(private searchService: SearchService/*, private requestService: RequestService, private notificationService: NotificationService*/) {
        this.searchChanged
            .debounceTime(600) // Wait Xms afterthe last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.clearResults();
                    return;
                }
                this.searchService.searchTv(this.searchText).subscribe(x => {
                    this.tvResults = x;
                });
            });
    }

    ngOnInit(): void {
        this.searchText = "";
        this.tvResults = [];
        this.result = {
            message: "",
            requestAdded: false
        }
    }

    search(text: any) {
        this.searchChanged.next(text.target.value);
    }

   

    private clearResults() {
        this.tvResults = [];
    }

}