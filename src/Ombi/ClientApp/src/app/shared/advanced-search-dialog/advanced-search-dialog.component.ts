import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { IDiscoverModel } from "../../interfaces";
import { SearchV2Service } from "../../services";


@Component({
  selector: "advanced-search-dialog",
  templateUrl: "advanced-search-dialog.component.html",
  styleUrls: [ "advanced-search-dialog.component.scss" ]
})
export class AdvancedSearchDialogComponent implements OnInit {
  constructor(
    public dialogRef: MatDialogRef<AdvancedSearchDialogComponent, string>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private fb: FormBuilder,
    private searchService: SearchV2Service,
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
    const watchProviderIds = <number[]>this.form.controls.watchProviders.value.map(x => x.provider_id);
    const genres = <number[]>this.form.controls.genreIds.value.map(x => x.id);
    await this.searchService.advancedSearch({
      watchProviders: watchProviderIds,
      genreIds: genres,
      type: this.form.controls.type.value,
    }, 0, 30);
  }
}
