import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service } from "../../../services";
import { IActorCredits } from "../../../interfaces/ISearchTvResultV2";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType } from "../../../interfaces";
import { AuthService } from "../../../auth/auth.service";

@Component({
    templateUrl: "./discover-actor.component.html",
    styleUrls: ["./discover-actor.component.scss"],
})
export class DiscoverActorComponent {
    public actorId: number;
    public actorCredits: IActorCredits;
    public loadingFlag: boolean;
    public isAdmin: boolean;

    public discoverResults: IDiscoverCardResult[] = [];

    constructor(private searchService: SearchV2Service,
        private route: ActivatedRoute,
        private auth: AuthService) {
        this.route.params.subscribe((params: any) => {
            this.actorId = params.actorId;
            this.isAdmin = this.auth.isAdmin();
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
