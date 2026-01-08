import { Component, OnInit, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from "@angular/material/dialog";
import { IUpdateModel } from "../../interfaces";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { TranslateModule } from "@ngx-translate/core";
import { OmbiDatePipe } from "app/pipes/OmbiDatePipe";


@Component({
        standalone: true,
  templateUrl: "update-dialog.component.html",
  styleUrls: [ "update-dialog.component.scss" ],
  imports: [
    CommonModule,
    TranslateModule,
    MatDialogModule,
    MatButtonModule,
    OmbiDatePipe

  ]
})
export class UpdateDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<UpdateDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: IUpdateModel
  ) { }

}
