import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormControl, UntypedFormGroup } from "@angular/forms";
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from "@angular/material/autocomplete";
import { MatChipsModule } from "@angular/material/chips";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { TranslateModule } from "@ngx-translate/core";
import { IMovieDbKeyword, IWatchProvidersResults } from "../../../interfaces";
import { debounceTime, distinctUntilChanged, map, startWith, switchMap } from "rxjs/operators";

import { Observable } from "rxjs";
import { TheMovieDbService } from "../../../services";

@Component({
    standalone: true,
  selector: "watch-providers-select",
  templateUrl: "watch-providers-select.component.html",
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
  @Input() public form: UntypedFormGroup;

  public watchProviders: IWatchProvidersResults[] = [];
  public control = new UntypedFormControl();
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
