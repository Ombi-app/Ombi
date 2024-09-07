import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";

@Component({
    selector: "youtube-trailer",
    templateUrl: "./youtube-trailer.component.html",
})
export class YoutubeTrailerComponent {
    
    constructor(
        public dialogRef: MatDialogRef<YoutubeTrailerComponent>,
        @Inject(MAT_DIALOG_DATA) public youtubeLink: string) {}
    
}
