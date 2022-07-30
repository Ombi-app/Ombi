import { Component, OnInit } from "@angular/core";
import {
  debounceTime,
  switchMap,
  tap,
  finalize,
} from "rxjs/operators";

import { empty} from "rxjs";
import { Router } from "@angular/router";
import { UntypedFormGroup, UntypedFormBuilder } from "@angular/forms";

@Component({
  selector: "app-nav-search",
  templateUrl: "./nav-search.component.html",
  styleUrls: ["./nav-search.component.scss"],
})
export class NavSearchComponent implements OnInit {

  public searchForm: UntypedFormGroup;

  constructor(
    private router: Router,
    private fb: UntypedFormBuilder
  ) {}

  public async ngOnInit() {
    this.searchForm = this.fb.group({
      input: null,
    });

    this.searchForm
      .get("input")
      .valueChanges.pipe(
        debounceTime(1300),
        switchMap((value: string) => {
          if (value) {
            this.router.navigate([`discover`, value]);
          }
          return empty();;
        })
      )
      .subscribe();
  }
}
