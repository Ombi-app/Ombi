import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { IAdvancedData } from "../../../../../interfaces";

@Component({
    templateUrl: "./movie-advanced-options.component.html",
    selector: "movie-advanced-options",
})
export class MovieAdvancedOptionsComponent {
    
    constructor(public dialogRef: MatDialogRef<MovieAdvancedOptionsComponent>, @Inject(MAT_DIALOG_DATA) public data: IAdvancedData,
               ) {
                 }
}
