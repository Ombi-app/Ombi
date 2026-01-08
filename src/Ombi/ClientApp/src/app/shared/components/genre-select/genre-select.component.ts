import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormControl, UntypedFormGroup } from "@angular/forms";
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { MatChipsModule } from "@angular/material/chips";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { TranslateModule } from "@ngx-translate/core";
import { debounceTime, distinctUntilChanged, map, startWith, switchMap } from "rxjs/operators";

import { IMovieDbKeyword } from "../../../interfaces";
import { Observable } from "rxjs";
import { SearchV2Service } from "../../../services";

@Component({
    standalone: true,
  selector: "genre-select",
  templateUrl: "genre-select.component.html",
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
export class GenreSelectComponent {
  constructor(
    private searchService: SearchV2Service
  ) {}

  @Input() public form: UntypedFormGroup;

  private _mediaType: string;
  @Input() set mediaType(type: string) {
    this._mediaType = type;
    this.searchService.getGenres(this._mediaType).subscribe((res) => {
      this.genres = res;
      this.filteredKeywords = this.control.valueChanges.pipe(
        startWith(''),
        map((genre: string | null) => genre ? this._filter(genre) : this.genres.slice()));
    });

  }
  get mediaType(): string {
    return this._mediaType;
  }
  public genres: IMovieDbKeyword[] = [];
  public control = new UntypedFormControl();
  public filteredTags: IMovieDbKeyword[];
  public filteredKeywords: Observable<IMovieDbKeyword[]>;

  @ViewChild('keywordInput') input: ElementRef<HTMLInputElement>;

   remove(word: IMovieDbKeyword): void {
    const exisiting = this.form.controls.genreIds.value;
    const index = exisiting.indexOf(word);

    if (index >= 0) {
      exisiting.splice(index, 1);
      this.form.controls.genreIds.setValue(exisiting);
    }
  }


  selected(event: MatAutocompleteSelectedEvent): void {
    const val = event.option.value;
    const exisiting = this.form.controls.genreIds.value;
    if(exisiting.indexOf(val) < 0) {
      exisiting.push(val);
    }
    this.form.controls.genreIds.setValue(exisiting);
    this.input.nativeElement.value = '';
    this.control.setValue(null);
  }

  private _filter(value: string|IMovieDbKeyword): IMovieDbKeyword[] {
    if (typeof value === 'object') {
      const filterValue = value.name.toLowerCase();
      return this.genres.filter(g => g.name.toLowerCase().includes(filterValue));
    } else if (typeof value === 'string') {
      const filterValue = value.toLowerCase();
      return this.genres.filter(g => g.name.toLowerCase().includes(filterValue));
    }

    return this.genres;
  }


}
