import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service, RequestService, MessageService } from "../../services";
import { IActorCredits } from "../../interfaces/ISearchTvResultV2";
import { IDiscoverCardResult } from "../interfaces";
import { RequestType } from "../../interfaces";

@Component({
    templateUrl: "./discover-actor.component.html",
    styleUrls: ["./discover-actor.component.scss"],
})
export class DiscoverActorComponent {
    public actorId: number;
    public actorCredits: IActorCredits;
    public loadingFlag: boolean;

    public discoverResults: IDiscoverCardResult[] = [];

    constructor(private searchService: SearchV2Service,
        private route: ActivatedRoute,
        private requestService: RequestService,
        private messageService: MessageService) {
        this.route.params.subscribe((params: any) => {
            this.actorId = params.actorId;
            this.loading();
            this.searchService.getMoviesByActor(this.actorId).subscribe(res => {
                this.actorCredits = res;
                this.createModel();
            });
        });
    }

    private createModel() {
        this.finishLoading();
        this.discoverResults = [];
        this.actorCredits.cast.forEach(m => {
            this.discoverResults.push({
                available: false, // TODO
                posterPath: `https://image.tmdb.org/t/p/w300/${m.poster_path}`,
                requested: false, // TODO
                title: m.title,
                type: RequestType.movie,
                id: m.id,
                url: null, // TODO
                rating: 0,
                overview: m.overview,
                approved: false // TODO
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
