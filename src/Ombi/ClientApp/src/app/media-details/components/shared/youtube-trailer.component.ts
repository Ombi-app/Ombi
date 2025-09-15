import { Component, Inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";

@Component({
        standalone: true,
    selector: "youtube-trailer",
    templateUrl: "./youtube-trailer.component.html",
    imports: [
        CommonModule
    ]
})
export class YoutubeTrailerComponent {
    
    constructor(
        public dialogRef: MatDialogRef<YoutubeTrailerComponent>,
        @Inject(MAT_DIALOG_DATA) public youtubeLink: string) {}
    
}
