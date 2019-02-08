import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { IDiscoverCardResult } from "../interfaces";
import { SearchV2Service } from "../../services";
import { dataURLToBlob } from "blob-util";
import { RequestType } from "../../interfaces";
import { ISearchMovieResultV2 } from "../../interfaces/ISearchMovieResultV2";

@Component({
    selector: "discover-card-details",
    templateUrl: "./discover-card-details.component.html",
    styleUrls: ["./discover-card-details.component.scss"],
})
export class DiscoverCardDetailsComponent implements OnInit {

    public result: ISearchMovieResultV2;

    constructor(
        public dialogRef: MatDialogRef<DiscoverCardDetailsComponent>,
        @Inject(MAT_DIALOG_DATA) public data: IDiscoverCardResult, private searchService: SearchV2Service) { }
        
        public async ngOnInit() {
            if (this.data.type === RequestType.movie) {
                this.result = await this.searchService.getFullMovieDetailsPromise(this.data.id);
            }
        }

      public onNoClick(): void {
        this.dialogRef.close();
      }
}
