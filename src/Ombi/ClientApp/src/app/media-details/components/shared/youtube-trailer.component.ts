import { Component, Inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from "@angular/material/dialog";
import { TranslateModule } from "@ngx-translate/core";
import { SafePipe } from "app/pipes/SafePipe";

@Component({
        standalone: true,
    selector: "youtube-trailer",
    templateUrl: "./youtube-trailer.component.html",
    imports: [
        CommonModule,
        TranslateModule,
        MatDialogModule,
        SafePipe
    ]
})
export class YoutubeTrailerComponent {
    
    constructor(
        public dialogRef: MatDialogRef<YoutubeTrailerComponent>,
        @Inject(MAT_DIALOG_DATA) public youtubeLink: string) {}
    
}
