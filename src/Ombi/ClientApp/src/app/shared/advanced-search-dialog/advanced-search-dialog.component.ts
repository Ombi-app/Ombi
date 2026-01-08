import { Component, Inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, UntypedFormBuilder, UntypedFormGroup } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatRadioModule } from "@angular/material/radio";
import { TranslateModule } from "@ngx-translate/core";
import { RequestType } from "../../interfaces";
import { SearchV2Service } from "../../services";
import { AdvancedSearchDialogDataService } from "./advanced-search-dialog-data.service";
import { GenreSelectComponent } from "../components/genre-select/genre-select.component";
import { KeywordSearchComponent } from "../components/keyword-search/keyword-search.component";
import { WatchProvidersSelectComponent } from "../components/watch-providers-select/watch-providers-select.component";

@Component({
    standalone: true,
    selector: "advanced-search-dialog",
    templateUrl: "advanced-search-dialog.component.html",
    styleUrls: [ "advanced-search-dialog.component.scss" ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatRadioModule,
        TranslateModule,
        GenreSelectComponent,
        KeywordSearchComponent,
        WatchProvidersSelectComponent
    ]
})
export class AdvancedSearchDialogComponent implements OnInit {
  constructor(
    public dialogRef: MatDialogRef<AdvancedSearchDialogComponent, boolean>,
    private fb: UntypedFormBuilder,
    private searchService: SearchV2Service,
    private advancedSearchDialogService: AdvancedSearchDialogDataService
  ) {}

  public form: UntypedFormGroup;
  public decades: number[] = [];

  public async ngOnInit() {
    const currentYear = new Date().getFullYear();
    const startDecade = Math.floor(currentYear / 10) * 10;
    for (let i = startDecade; i >= 1900; i -= 10) {
        this.decades.push(i);
    }

    this.form = this.fb.group({
        keywordIds: [[]],
        genreIds: [[]],
        releaseYear: [],
        decade: [],
        type: ['movie'],
        watchProviders: [[]],
    })

    this.form.controls.type.valueChanges.subscribe(val => {
      this.form.controls.genres.setValue([]);
      this.form.controls.watchProviders.setValue([]);
    });
  }

  public async onSubmit() {
    const formData = this.form.value;
    const watchProviderIds = <number[]>formData.watchProviders.map(x => x.provider_id);
    const genres = <number[]>formData.genreIds.map(x => x.id);
    const keywords = <number[]>formData.keywordIds.map(x => x.id);
    const data = await this.searchService.advancedSearch({
      watchProviders: watchProviderIds,
      genreIds: genres,
      keywordIds: keywords,
      releaseYear: formData.releaseYear,
      decade: formData.decade,
      type: formData.type,
    }, 0, 30);

    const type = formData.type === 'movie' ? RequestType.movie : RequestType.tvShow;
    this.advancedSearchDialogService.setData(data, type);
    this.advancedSearchDialogService.setOptions(watchProviderIds, genres, keywords, formData.releaseYear, type, 30);

    this.dialogRef.close(true);
  }

  public onClose() {
    this.dialogRef.close(false);
  }

}
