import { Component, Input, OnInit } from "@angular/core";
import {
  debounceTime,
  switchMap,
  tap,
  finalize,
} from "rxjs/operators";

import { empty} from "rxjs";
import { IMultiSearchResult } from "../interfaces";
import { Router } from "@angular/router";
import { FormGroup, FormBuilder } from "@angular/forms";
import { MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { SearchFilter } from "./SearchFilter";
import { FilterService } from "../discover/services/filter-service";

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
        debounceTime(1300),
        tap(() => (this.searching = true)),
        switchMap((value: string) => {
          if (value) {
            this.router.navigate([`discover`, value]);
            // return this.searchService
            //   .multiSearch(value, this.filter)
            //   .pipe(finalize(() => (this.searching = false)));
          }
          return empty().pipe(finalize(() => (this.searching = false)));
        })
      )
      .subscribe((r) => (this.results = r));
  }

  public selected(event: MatAutocompleteSelectedEvent) {
    this.searchForm.controls.input.setValue(null);
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
