import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule, UntypedFormGroup, UntypedFormBuilder } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatIconModule } from "@angular/material/icon";
import { Router } from "@angular/router";
import { TranslateModule } from "@ngx-translate/core";
import {
  debounceTime,
  switchMap,
  tap,
  finalize,
} from "rxjs/operators";

import { empty} from "rxjs";

@Component({
    standalone: true,
    selector: "app-nav-search",
    templateUrl: "./nav-search.component.html",
    styleUrls: ["./nav-search.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        TranslateModule
    ]
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
