import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { SearchService } from '../services/search.service';

import { IMovieResult } from './interfaces/IMovieResult';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './search.component.html',
    providers: [SearchService]
})
export class SearchComponent implements OnInit {

    searchText: string;
    searchChanged: Subject<string> = new Subject<string>();
    movieResults: IMovieResult[];

    constructor(private searchService: SearchService) {
        //this.searchChanged
        //    .debounceTime(300) // wait 300ms after the last event before emitting last event
        //    .distinctUntilChanged() // only emit if value is different from previous value
        //    .subscribe(x => {
        //        this.searchText = x as string;
        //        
        //    });
    }

    ngOnInit(): void {
        this.searchText = "";
        this.movieResults = [];
    }

    search(text: any) {
        //this.searchChanged.next(text);
        this.searchService.searchMovie(text.target.value).subscribe(x => this.movieResults = x);
    }

}