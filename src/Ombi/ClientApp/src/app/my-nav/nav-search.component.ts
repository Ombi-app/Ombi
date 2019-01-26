import { Component, Input } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable, Subject } from 'rxjs';
import { map, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { SearchV2Service } from '../services/searchV2.service';
import { IMultiSearchResult } from '../interfaces';

@Component({
  selector: 'app-nav-search',
  templateUrl: './nav-search.component.html',
  styleUrls: ['./nav-search.component.scss']
})
export class NavSearchComponent {

  public searchChanged: Subject<string> = new Subject<string>();
  public searchText: string;
  public searchResult: IMultiSearchResult[];
  public option: IMultiSearchResult;

  constructor(private searchService: SearchV2Service) {
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
}
