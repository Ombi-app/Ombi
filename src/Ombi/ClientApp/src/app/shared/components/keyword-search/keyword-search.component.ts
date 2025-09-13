import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormControl, UntypedFormGroup } from "@angular/forms";
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { MatChipsModule } from "@angular/material/chips";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { TranslateModule } from "@ngx-translate/core";
import { debounceTime, distinctUntilChanged, startWith, switchMap } from "rxjs/operators";

import { IMovieDbKeyword } from "../../../interfaces";
import { Observable } from "rxjs";
import { TheMovieDbService } from "../../../services";

@Component({
    standalone: true,
  selector: "keyword-search",
  templateUrl: "keyword-search.component.html",
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    TranslateModule
  ]
})
export class KeywordSearchComponent implements OnInit  {
  constructor(
    private tmdbService: TheMovieDbService
  ) {}

  @Input() public form: UntypedFormGroup;
  public control = new UntypedFormControl();
  public filteredTags: IMovieDbKeyword[];
  public filteredKeywords: Observable<IMovieDbKeyword[]>;

  @ViewChild('keywordInput') input: ElementRef<HTMLInputElement>;

  ngOnInit(): void {

    this.filteredKeywords = this.control.valueChanges.pipe(
      startWith(''),
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(val => {
        return this.filter(val || '')
      })
    );
  }

  filter(val: string): Observable<any[]> {
    return this.tmdbService.getKeywords(val);
   };

   remove(word: IMovieDbKeyword): void {
    const exisiting = this.form.controls.keywordIds.value;
    const index = exisiting.indexOf(word);

    if (index >= 0) {
      exisiting.splice(index, 1);
      this.form.controls.keywordIds.setValue(exisiting);
    }
  }


  selected(event: MatAutocompleteSelectedEvent): void {
    const val = event.option.value;
    const exisiting = this.form.controls.keywordIds.value;    
    if (exisiting.indexOf(val) < 0) {
        exisiting.push(val);
    }
    this.form.controls.keywordIds.setValue(exisiting);
    this.input.nativeElement.value = '';
    this.control.setValue(null);
  }

}
