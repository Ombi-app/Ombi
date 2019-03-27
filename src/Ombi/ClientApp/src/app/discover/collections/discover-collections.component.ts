import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service } from "../../services";
import { IMovieCollectionsViewModel } from "../../interfaces/ISearchTvResultV2";

@Component({
    templateUrl: "./discover-collections.component.html",
    styleUrls: ["./discover-collections.component.scss"],
})
export class DiscoverCollectionsComponent implements OnInit {

    public collectionId: number;
    public collection: IMovieCollectionsViewModel;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute) {
        this.route.params.subscribe((params: any) => {
            this.collectionId = params.collectionId;
        });
     }

    public async ngOnInit() {
       this.collection = await this.searchService.getMovieCollections(this.collectionId);
    }
}
