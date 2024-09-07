import { Component, OnInit } from "@angular/core";
import { MessageService, SearchV2Service } from "../../../services";

import { TranslateService } from "@ngx-translate/core";
import { ActivatedRoute } from "@angular/router";
import { AuthService } from "../../../auth/auth.service";
import { IDiscoverCardResult } from "../../interfaces";
import { IMovieCollectionsViewModel } from "../../../interfaces/ISearchTvResultV2";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { RequestType } from "../../../interfaces";
import { FeaturesFacade } from "../../../state/features/features.facade";

@Component({
    templateUrl: "./discover-collections.component.html",
    styleUrls: ["./discover-collections.component.scss"],
})
export class DiscoverCollectionsComponent implements OnInit {

    public collectionId: number;
    public collection: IMovieCollectionsViewModel;
    public loadingFlag: boolean;
    public isAdmin: boolean;
    public is4kEnabled = false;

    public discoverResults: IDiscoverCardResult[] = [];

    constructor(private searchService: SearchV2Service,
         private route: ActivatedRoute,
         private requestServiceV2: RequestServiceV2,
         private messageService: MessageService,
         private auth: AuthService,
         private translate: TranslateService,
         private featureFacade: FeaturesFacade) {
        this.route.params.subscribe((params: any) => {
            this.collectionId = params.collectionId;
        });
     }

    public async ngOnInit() {
        this.is4kEnabled = this.featureFacade.is4kEnabled();
        this.loadingFlag = true;
        this.isAdmin = this.auth.isAdmin();
        this.collection = await this.searchService.getMovieCollections(this.collectionId);
        this.createModel();
    }

    public async requestCollection() {
        this.loading();
        this.requestServiceV2.requestMovieCollection(this.collectionId).subscribe(result => {
            if (result.result) {
                this.messageService.send(this.translate.instant("Requests.CollectionSuccesfullyAdded", { name: this.collection.name }));
            } else {
                this.messageService.sendRequestEngineResultError(result);
            }
            this.finishLoading();
        });
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
