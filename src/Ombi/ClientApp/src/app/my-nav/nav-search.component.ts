import { Component, OnInit } from "@angular/core";
import { Observable } from "rxjs";
import {
  debounceTime,
  distinctUntilChanged,
  switchMap,
  tap,
  finalize,
} from "rxjs/operators";

import { empty, of } from "rxjs";
import { SearchV2Service } from "../services/searchV2.service";
import { IMultiSearchResult } from "../interfaces";
import { Router } from "@angular/router";
import { NgbTypeaheadSelectItemEvent } from "@ng-bootstrap/ng-bootstrap";
import { FormGroup, FormBuilder } from "@angular/forms";
import { MatAutocompleteSelectedEvent } from "@angular/material";

@Component({
  selector: "app-nav-search",
  templateUrl: "./nav-search.component.html",
  styleUrls: ["./nav-search.component.scss"],
})
export class NavSearchComponent implements OnInit {
  public selectedItem: string;
  public results: IMultiSearchResult[];
  public searching = false;

  public searchForm: FormGroup;

  constructor(
    private searchService: SearchV2Service,
    private router: Router,
    private fb: FormBuilder
  ) {}

  public async ngOnInit() {
    this.searchForm = this.fb.group({
      input: null,
    });

    this.searchForm
      .get("input")
      .valueChanges.pipe(
        debounceTime(600),
        tap(() => (this.searching = true)),
        switchMap((value: string) => {
          if (value) {
            return this.searchService
              .multiSearch(value)
              .pipe(finalize(() => (this.searching = false)));
          }
          return empty().pipe(finalize(() => (this.searching = false)));
        })
      )
      .subscribe((r) => (this.results = r));
  }

  public selected(event: MatAutocompleteSelectedEvent) {
      const val = event.option.value as IMultiSearchResult;
    if (val.mediaType == "movie") {
      this.router.navigate([`details/movie/${val.id}`]);
      return;
    } else if (val.mediaType == "tv") {
      this.router.navigate([`details/tv/${val.id}/true`]);
      return;
    } else if (val.mediaType == "person") {
      this.router.navigate([`discover/actor/${val.id}`]);
      return;
    } else if (val.mediaType == "Artist") {
      this.router.navigate([`details/artist/${val.id}`]);
      return;
    }
  }
  displayFn(result: IMultiSearchResult) {
    if (result) { return result.title; }
  }
}
