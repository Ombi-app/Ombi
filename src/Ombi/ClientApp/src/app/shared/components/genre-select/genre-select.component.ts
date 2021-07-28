import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { FormControl, FormGroup } from "@angular/forms";
import { debounceTime, distinctUntilChanged, map, startWith, switchMap } from "rxjs/operators";

import { IMovieDbKeyword } from "../../../interfaces";
import { MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { Observable } from "rxjs";
import { TheMovieDbService } from "../../../services";

@Component({
  selector: "genre-select",
  templateUrl: "genre-select.component.html"
})
export class GenreSelectComponent {
  constructor(
    private tmdbService: TheMovieDbService
  ) {}

  @Input() public form: FormGroup;

  private _mediaType: string;
  @Input() set mediaType(type: string) {
    this._mediaType = type;
    this.tmdbService.getGenres(this._mediaType).subscribe((res) => {
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
  public control = new FormControl();
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
