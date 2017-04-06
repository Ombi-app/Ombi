import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { SearchService } from '../services/search.service';
import { RequestService } from '../services/request.service';

import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './search.component.html',
    providers: [SearchService, RequestService]
})
export class SearchComponent implements OnInit {

    searchText: string;
    searchChanged: Subject<string> = new Subject<string>();
    movieResults: ISearchMovieResult[];
    result: IRequestEngineResult;

    constructor(private searchService: SearchService, private requestService: RequestService) {
        this.searchChanged
            .debounceTime(600) // Wait Xms afterthe last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.clearResults();
                    return;
                }
                this.searchService.searchMovie(this.searchText).subscribe(x => this.movieResults = x);
            });
    }

    ngOnInit(): void {
        this.searchText = "";
        this.movieResults = [];
        this.result = {
            message: "",
            requestAdded:false
        }
    }

    search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    request(searchResult: ISearchMovieResult) {
        this.requestService.requestMovie(searchResult).subscribe(x => this.result = x);
    }

    popularMovies() {
        this.clearResults();
        this.searchService.popularMovies().subscribe(x => this.movieResults = x);
    }
    nowPlayingMovies() {
        this.clearResults();
        this.searchService.nowPlayingMovies().subscribe(x => this.movieResults = x);
    }
    topRatedMovies() {
        this.clearResults();
        this.searchService.topRatedMovies().subscribe(x => this.movieResults = x);
    }
    upcomingMovies() {
        this.clearResults();
        this.searchService.upcomignMovies().subscribe(x => this.movieResults = x);
    }

    private clearResults() {
        this.movieResults = [];
    }

}