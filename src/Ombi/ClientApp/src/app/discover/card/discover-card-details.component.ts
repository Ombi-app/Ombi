import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { IDiscoverCardResult } from "../interfaces";

@Component({
    selector: "discover-card-details",
    templateUrl: "./discover-card-details.component.html",
    styleUrls: ["./discover-card-details.component.scss"],
})
export class DiscoverCardDetailsComponent {
    constructor(
        public dialogRef: MatDialogRef<DiscoverCardDetailsComponent>,
        @Inject(MAT_DIALOG_DATA) public data: IDiscoverCardResult) { }
    
      onNoClick(): void {
        this.dialogRef.close();
      }
}
