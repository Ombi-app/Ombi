import { Component, Inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";


@Component({
  selector: "advanced-search-dialog",
  templateUrl: "advanced-search-dialog.component.html",
  styleUrls: [ "advanced-search-dialog.component.scss" ]
})
export class AdvancedSearchDialogComponent implements OnInit {
  constructor(
    public dialogRef: MatDialogRef<AdvancedSearchDialogComponent, string>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private fb: FormBuilder
  ) {}

  public form: FormGroup;


  public async ngOnInit() {

    this.form = this.fb.group({
        keywords: [[]],
        genres: [[]],
        releaseYear: [],
        type: ['movie'],
    })

    this.form.controls.type.valueChanges.subscribe(val => {
      this.form.controls.genres.setValue([]);
    });


  }
}
