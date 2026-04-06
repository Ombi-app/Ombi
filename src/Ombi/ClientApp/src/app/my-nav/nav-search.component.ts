import { Component, OnInit, ViewChild, ElementRef, DestroyRef, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormGroup, UntypedFormBuilder } from "@angular/forms";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Router } from "@angular/router";
import { TranslateModule } from "@ngx-translate/core";
import {
  debounceTime,
  switchMap,
} from "rxjs/operators";

import { EMPTY } from "rxjs";

@Component({
    standalone: true,
    selector: "app-nav-search",
    templateUrl: "./nav-search.component.html",
    styleUrls: ["./nav-search.component.scss"],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        TranslateModule
    ]
})
export class NavSearchComponent implements OnInit {

  @ViewChild('searchInput') searchInput: ElementRef<HTMLInputElement>;

  public searchForm: UntypedFormGroup;
  public isExpanded = false;

  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private router: Router,
    private fb: UntypedFormBuilder
  ) {}

  public ngOnInit() {
    this.searchForm = this.fb.group({
      input: null,
    });

    const inputControl = this.searchForm.get("input");
    if (inputControl) {
      inputControl.valueChanges.pipe(
        debounceTime(600),
        switchMap((value: string) => {
          const term = (value ?? '').trim();
          if (term) {
            this.router.navigate([`discover`, term]);
          }
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
    }
  }

  public toggleSearch(): void {
    if (this.isExpanded && !this.searchForm.get('input')?.value) {
      this.isExpanded = false;
    } else if (!this.isExpanded) {
      this.isExpanded = true;
      requestAnimationFrame(() => this.searchInput?.nativeElement?.focus());
    }
  }

  public onBlur(): void {
    if (!this.searchForm.get('input')?.value) {
      this.isExpanded = false;
    }
  }

  public clearSearch(): void {
    this.searchForm.get('input')?.setValue('');
    this.searchInput?.nativeElement?.focus();
  }
}
