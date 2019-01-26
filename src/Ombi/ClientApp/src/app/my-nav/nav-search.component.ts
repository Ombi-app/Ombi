import { Component, Input } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { SearchV2Service } from '../services/searchV2.service';
import { IMultiSearchResult } from '../interfaces';
import { MatAutocompleteSelectedEvent } from '@angular/material';
import { Router } from '@angular/router';

@Component({
    selector: 'app-nav-search',
    templateUrl: './nav-search.component.html',
    styleUrls: ['./nav-search.component.scss']
})
export class NavSearchComponent {

    public searchChanged: Subject<string> = new Subject<string>();
    public searchText: string;
    public searchResult: IMultiSearchResult[];

    constructor(private searchService: SearchV2Service, private router: Router) {
        this.searchChanged.pipe(
            debounceTime(600), // Wait Xms after the last event before emitting last event
            distinctUntilChanged(), // only emit if value is different from previous value
        ).subscribe(x => {
            this.searchText = x as string;
            this.searchService.multiSearch(this.searchText).subscribe(x => this.searchResult = x)
        });
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public selected(option: MatAutocompleteSelectedEvent) {
        var selected = option.option.value as IMultiSearchResult;
        if (selected.media_type == "movie") {
            this.router.navigate([`details/movie/${selected.id}`]);
            return;
        }
    }
}
