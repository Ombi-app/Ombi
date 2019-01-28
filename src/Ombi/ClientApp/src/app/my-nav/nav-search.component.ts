import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';

import { SearchV2Service } from '../services/searchV2.service';
import { IMultiSearchResult } from '../interfaces';
import { Router } from '@angular/router';
import { NgbTypeaheadSelectItemEvent } from '@ng-bootstrap/ng-bootstrap';

@Component({
    selector: 'app-nav-search',
    templateUrl: './nav-search.component.html',
    styleUrls: ['./nav-search.component.scss']
})
export class NavSearchComponent {

    public selectedItem: string;

    
    public searching = false;
    public searchFailed = false;
    
    
    public formatter = (result: IMultiSearchResult) => result.media_type == "movie" ? result.title : result.name;
    
    public searchModel = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(600),
      distinctUntilChanged(),
      switchMap(term =>
        this.searchService.multiSearch(term)
      )
    )

    constructor(private searchService: SearchV2Service, private router: Router) {

    }

  

    public selected(event: NgbTypeaheadSelectItemEvent) {
        if (event.item.media_type == "movie") {
            this.router.navigate([`details/movie/${event.item.id}`]);
            return;
        }
    }
}
