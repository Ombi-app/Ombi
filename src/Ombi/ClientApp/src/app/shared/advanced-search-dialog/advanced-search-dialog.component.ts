import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { RequestType } from "../../interfaces";
import { SearchV2Service } from "../../services";
import { AdvancedSearchDialogDataService } from "./advanced-search-dialog-data.service";

@Component({
  selector: "advanced-search-dialog",
  templateUrl: "advanced-search-dialog.component.html",
  styleUrls: [ "advanced-search-dialog.component.scss" ]
})
export class AdvancedSearchDialogComponent implements OnInit {
  constructor(
    public dialogRef: MatDialogRef<AdvancedSearchDialogComponent, boolean>,
    private fb: FormBuilder,
    private searchService: SearchV2Service,
    private advancedSearchDialogService: AdvancedSearchDialogDataService
  ) {}

  public form: FormGroup;

  public async ngOnInit() {

    this.form = this.fb.group({
        keywordIds: [[]],
        genreIds: [[]],
        releaseYear: [],
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
      type: formData.type,
    }, 0, 30);

    this.advancedSearchDialogService.setData(data, formData.type === 'movie' ? RequestType.movie : RequestType.tvShow);

    this.dialogRef.close(true);
  }

  public onClose() {
    this.dialogRef.close(false);
  }

}
