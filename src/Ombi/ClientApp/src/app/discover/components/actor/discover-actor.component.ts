import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service } from "../../../services";
import { IActorCredits, IActorCast } from "../../../interfaces/ISearchTvResultV2";
import { IDiscoverCardResult } from "../../interfaces";
import { RequestType } from "../../../interfaces";
import { AuthService } from "../../../auth/auth.service";
import { FilterService } from "../../services/filter-service";
import { SearchFilter } from "../../../my-nav/SearchFilter";
import { forkJoin } from "rxjs";
import { isEqual } from "lodash";

@Component({
    templateUrl: "./discover-actor.component.html",
    styleUrls: ["./discover-actor.component.scss"],
})
export class DiscoverActorComponent {
    public actorId: number;
    public loadingFlag: boolean;
    public isAdmin: boolean;

    public discoverResults: IDiscoverCardResult[] = [];
    public filter: SearchFilter;

    constructor(private searchService: SearchV2Service,
        private route: ActivatedRoute,
        private filterService: FilterService,
        private auth: AuthService) {
            
        this.filter = this.filterService.filter;
        this.route.params.subscribe((params: any) => {
            this.actorId = params.actorId;
            this.isAdmin = this.auth.isAdmin();
            this.search();
        });
    }

    public async ngOnInit() {
        this.filterService.onFilterChange.subscribe(async x => {
            if (!isEqual(this.filter, x)) {
                this.filter = { ...x };
                await this.search();
            }
        });
    }

    private search() {
        this.discoverResults = [];
        this.loading();

        var searches = [];
        if (this.filter.movies) {
            var moviesSearch = this.searchService.getMoviesByActor(this.actorId);
            moviesSearch.subscribe(res => {
                this.pushDiscoverResults(res.cast, RequestType.movie);
            });
            searches.push(moviesSearch);
        }

        if (this.filter.tvShows) {
            var TvSearch = this.searchService.getTvByActor(this.actorId);
            TvSearch.subscribe(res => {
                this.pushDiscoverResults(res.cast, RequestType.tvShow);
            });
            searches.push(TvSearch);
        }

        if (searches.length === 0) {
            this.finishLoading();
        }
        forkJoin(searches).subscribe(() => {
            this.finishLoading();
        });
    }

    pushDiscoverResults(cast: IActorCast[], type: RequestType) {
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
    }

    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }
}
