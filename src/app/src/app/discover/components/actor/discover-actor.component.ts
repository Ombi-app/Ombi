import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service } from "../../../services";
import { IActorCredits, IActorCast, IActorCrew } from "../../../interfaces/ISearchTvResultV2";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType } from "../../../interfaces";
import { AuthService } from "../../../auth/auth.service";
import { forkJoin } from "rxjs";
import { FeaturesFacade } from "../../../state/features/features.facade";

@Component({
    templateUrl: "./discover-actor.component.html",
    styleUrls: ["./discover-actor.component.scss"],
})
export class DiscoverActorComponent implements OnInit {
    public actorId: number;
    public loadingFlag: boolean;
    public isAdmin: boolean;
    public is4kEnabled = false;

    public discoverResults: IDiscoverCardResult[] = [];

    constructor(private searchService: SearchV2Service,
        private route: ActivatedRoute,
        private auth: AuthService,
        private featureService: FeaturesFacade) {
        this.route.params.subscribe((params: any) => {
            this.actorId = params.actorId;
        });
    }
    ngOnInit() {
        this.isAdmin = this.auth.isAdmin();
        this.is4kEnabled = this.featureService.is4kEnabled();
        this.discoverResults = [];
        this.loading();

        forkJoin([
            this.searchService.getMoviesByActor(this.actorId),
            this.searchService.getTvByActor(this.actorId)
        ]).subscribe(([movie, tv]) => {
            this.pushDiscoverResults(movie.crew, movie.cast, RequestType.movie);
            this.pushDiscoverResults(tv.crew, tv.cast, RequestType.tvShow);
            this.finishLoading();
        });
    }

    pushDiscoverResults(crew: IActorCrew[], cast: IActorCast[], type: RequestType) {
        cast.forEach(m => {
            this.discoverResults.push({
                available: false,
                posterPath: m.poster_path ? `https://image.tmdb.org/t/p/w300/${m.poster_path}` : "../../../images/default_movie_poster.png",
                requested: false,
                title: m.title,
                type: type,
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
        crew.forEach(m => {
            this.discoverResults.push({
                available: false,
                posterPath: m.poster_path ? `https://image.tmdb.org/t/p/w300/${m.poster_path}` : "../../../images/default_movie_poster.png",
                requested: false,
                title: m.title,
                type: type,
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
