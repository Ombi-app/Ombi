import { Component, OnInit, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { IUpdateModel } from "../../interfaces";


@Component({
  templateUrl: "update-dialog.component.html",
  styleUrls: [ "update-dialog.component.scss" ]
})
export class UpdateDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<UpdateDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: IUpdateModel
  ) { }

}
