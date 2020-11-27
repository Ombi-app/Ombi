import { Component, AfterViewInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service } from "../../../services";
import { IActorCredits } from "../../../interfaces/ISearchTvResultV2";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType } from "../../../interfaces";

@Component({
    templateUrl: "./discover-actor.component.html",
    styleUrls: ["./discover-actor.component.scss"],
})
export class DiscoverActorComponent implements AfterViewInit {
    public actorId: number;
    public actorCredits: IActorCredits;
    public loadingFlag: boolean;

    public discoverResults: IDiscoverCardResult[] = [];
    
    constructor(private searchService: SearchV2Service,
        private route: ActivatedRoute) {
        this.route.params.subscribe((params: any) => {
            this.actorId = params.actorId;
            this.loading();
            this.searchService.getMoviesByActor(this.actorId).subscribe(res => {
                this.actorCredits = res;
                this.createModel();
            });
        });
    }

    public async ngAfterViewInit() {
        // this.discoverResults.forEach((result) => {
        //     this.searchService.getFullMovieDetails(result.id).subscribe(x => {
        //         result.available = x.available;
        //         result.approved = x.approved;
        //         result.rating = x.voteAverage;
        //         result.requested = x.requested;
        //         result.url = x.homepage;
        //     });
        // });
    }

    private createModel() {
        this.finishLoading();
        this.discoverResults = [];
        this.actorCredits.cast.forEach(m => {
            this.discoverResults.push({
                available: false,
                posterPath: m.poster_path ? `https://image.tmdb.org/t/p/w300/${m.poster_path}` : "../../../images/default_movie_poster.png",
                requested: false,
                title: m.title,
                type: RequestType.movie,
                id: m.id,
                url: null,
                rating: 0,
                overview: m.overview,
                approved: false,
                imdbid: "",
                denied: false,
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
