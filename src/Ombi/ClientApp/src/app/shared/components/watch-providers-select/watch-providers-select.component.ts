import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { FormControl, FormGroup } from "@angular/forms";
import { IMovieDbKeyword, IWatchProvidersResults } from "../../../interfaces";
import { debounceTime, distinctUntilChanged, map, startWith, switchMap } from "rxjs/operators";

import { MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { Observable } from "rxjs";
import { TheMovieDbService } from "../../../services";

@Component({
  selector: "watch-providers-select",
  templateUrl: "watch-providers-select.component.html"
})
export class WatchProvidersSelectComponent {
  constructor(
    private tmdbService: TheMovieDbService
  ) {}

  private _mediaType: string;
  @Input() set mediaType(type: string) {
    this._mediaType = type;
    this.tmdbService.getWatchProviders(this._mediaType).subscribe((res) => {
      this.watchProviders = res;
      this.filteredList = this.control.valueChanges.pipe(
        startWith(''),
        map((genre: string | null) => genre ? this._filter(genre) : this.watchProviders.slice()));
    });

  }
  get mediaType(): string {
    return this._mediaType;
  }
  @Input() public form: FormGroup;

  public watchProviders: IWatchProvidersResults[] = [];
  public control = new FormControl();
  public filteredTags: IWatchProvidersResults[];
  public filteredList: Observable<IWatchProvidersResults[]>;

  @ViewChild('keywordInput') input: ElementRef<HTMLInputElement>;


   remove(word: IWatchProvidersResults): void {
    const exisiting = this.form.controls.watchProviders.value;
    const index = exisiting.indexOf(word);

    if (index >= 0) {
      exisiting.splice(index, 1);
      this.form.controls.watchProviders.setValue(exisiting);
    }
  }


  selected(event: MatAutocompleteSelectedEvent): void {
    const val = event.option.value;
    const exisiting = this.form.controls.watchProviders.value;
    if (exisiting.indexOf(val) < 0) {
        exisiting.push(val);
    }
    this.form.controls.watchProviders.setValue(exisiting);
    this.input.nativeElement.value = '';
    this.control.setValue(null);
  }

  private _filter(value: string|IWatchProvidersResults): IWatchProvidersResults[] {
    if (typeof value === 'object') {
      const filterValue = value.provider_name.toLowerCase();
      return this.watchProviders.filter(g => g.provider_name.toLowerCase().includes(filterValue));
    } else if (typeof value === 'string') {
      const filterValue = value.toLowerCase();
      return this.watchProviders.filter(g => g.provider_name.toLowerCase().includes(filterValue));
    }

    return this.watchProviders;
  }

}
