import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ISearchMovieResultV2 } from "../interfaces/ISearchMovieResultV2";

@Component({
    selector: "movie-trailer",
    templateUrl: "./movie-details-trailer.component.html",
})
export class MovieDetailsTrailerComponent {
    
    constructor(
        public dialogRef: MatDialogRef<MovieDetailsTrailerComponent>,
        @Inject(MAT_DIALOG_DATA) public data: ISearchMovieResultV2) {}
    
}
