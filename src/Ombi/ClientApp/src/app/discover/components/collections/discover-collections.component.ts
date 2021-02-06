import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service, RequestService, MessageService } from "../../../services";
import { IMovieCollectionsViewModel } from "../../../interfaces/ISearchTvResultV2";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType } from "../../../interfaces";

@Component({
    templateUrl: "./discover-collections.component.html",
    styleUrls: ["./discover-collections.component.scss"],
})
export class DiscoverCollectionsComponent implements OnInit {

    public collectionId: number;
    public collection: IMovieCollectionsViewModel;
    public loadingFlag: boolean;
    
    public discoverResults: IDiscoverCardResult[] = [];

    constructor(private searchService: SearchV2Service,
         private route: ActivatedRoute,
         private requestService: RequestService,
         private messageService: MessageService) {
        this.route.params.subscribe((params: any) => {
            this.collectionId = params.collectionId;
        });
     }

    public async ngOnInit() {
        this.loadingFlag = true;
        this.collection = await this.searchService.getMovieCollections(this.collectionId);
        this.createModel();
    }

    public async requestCollection() {
        await this.collection.collection.forEach(async (movie) => {
            await this.requestService.requestMovie({theMovieDbId: movie.id, languageCode: null, requestOnBehalf: null}).toPromise();
        });
        this.messageService.send("Requested Collection");
    }

    private createModel() {
        this.finishLoading();
        this.collection.collection.forEach(m => {
            this.discoverResults.push({
                available: m.available,
                posterPath: `https://image.tmdb.org/t/p/w300/${m.posterPath}`,
                requested: m.requested,
                title: m.title,
                type: RequestType.movie,
                id: m.id,
                url: `http://www.imdb.com/title/${m.imdbId}/`,
                rating: 0,
                overview: m.overview,
                approved: m.approved,
                imdbid: m.imdbId,
                denied:false,
                background: ""
            });
        });
    }

    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }
}
