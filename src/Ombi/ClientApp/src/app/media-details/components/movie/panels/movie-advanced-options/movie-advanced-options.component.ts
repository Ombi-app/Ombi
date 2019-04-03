import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { IAdvancedModel, IAdvancedData } from "../../../../../interfaces";

@Component({
    templateUrl: "./movie-advanced-options.component.html",
    selector: "movie-advanced-options",
})
export class MovieAdvancedOptionsComponent {
    
    public options: IAdvancedModel;

    constructor(public dialogRef: MatDialogRef<MovieAdvancedOptionsComponent>, @Inject(MAT_DIALOG_DATA) public data: IAdvancedData) { }
}
