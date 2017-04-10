import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/map';

import { SearchService } from '../services/search.service';
import { RequestService } from '../services/request.service';
import { NotificationService } from '../services/notification.service';

import { ISearchMovieResult } from '../interfaces/ISearchMovieResult';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './search.component.html',
})
export class SearchComponent implements OnInit {

    searchText: string;
    searchChanged: Subject<string> = new Subject<string>();
    movieResults: ISearchMovieResult[];
    result: IRequestEngineResult;

    constructor(private searchService: SearchService, private requestService: RequestService, private notificationService : NotificationService) {
        this.searchChanged
            .debounceTime(600) // Wait Xms afterthe last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.clearResults();
                    return;
                }
                this.searchService.searchMovie(this.searchText).subscribe(x => {
                    this.movieResults = x;

                    // Now let's load some exta info including IMDBId
                    // This way the search is fast at displaying results.
                    this.getExtaInfo();
                });
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
        searchResult.requested = true;
        this.requestService.requestMovie(searchResult).subscribe(x => {
            this.result = x;

            if (this.result.requestAdded) {
                this.notificationService.success("Request Added",
                    `Request for ${searchResult.title} has been added successfully`);
            } else {
                this.notificationService.warning("Request Added", this.result.message);
            }
        });
    }

    popularMovies() {
        this.clearResults();
        this.searchService.popularMovies().subscribe(x => {
            this.movieResults = x;
            this.getExtaInfo();
        });
    }
    nowPlayingMovies() {
        this.clearResults();
        this.searchService.nowPlayingMovies().subscribe(x => {
            this.movieResults = x;
            this.getExtaInfo();
        });
    }
    topRatedMovies() {
        this.clearResults();
        this.searchService.topRatedMovies().subscribe(x => {
            this.movieResults = x;
            this.getExtaInfo();
        });
    }
    upcomingMovies() {
        this.clearResults();
        this.searchService.upcomignMovies().subscribe(x => {
            this.movieResults = x;
            this.getExtaInfo();
        });
    }

    private getExtaInfo() {
        this.searchService.extraInfo(this.movieResults).subscribe(m => this.movieResults = m);
    }

    private clearResults() {
        this.movieResults = [];
    }

}