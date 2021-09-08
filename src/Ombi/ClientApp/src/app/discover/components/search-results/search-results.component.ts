import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { Component, OnInit } from "@angular/core";
import { IMultiSearchResult, ISearchMovieResult, RequestType } from "../../../interfaces";

import { AdvancedSearchDialogDataService } from "../../../shared/advanced-search-dialog/advanced-search-dialog-data.service";
import { AuthService } from "../../../auth/auth.service";
import { FilterService } from "../../services/filter-service";
import { IDiscoverCardResult } from "../../interfaces";
import { SearchFilter } from "../../../my-nav/SearchFilter";
import { SearchV2Service } from "../../../services";
import { StorageService } from "../../../shared/storage/storage-service";
import { isEqual } from "lodash";

@Component({
    templateUrl: "./search-results.component.html",
    styleUrls: ["../discover/discover.component.scss"],
})
export class DiscoverSearchResultsComponent implements OnInit {

    public loadingFlag: boolean;
    public searchTerm: string;
    public results: IMultiSearchResult[];
    public isAdmin: boolean;

    public discoverResults: IDiscoverCardResult[] = [];

    public filter: SearchFilter;

    private isAdvancedSearch: boolean;

    constructor(private searchService: SearchV2Service,
        private route: ActivatedRoute,
        private filterService: FilterService,
        private router: Router,
        private advancedDataService: AdvancedSearchDialogDataService,
        private store: StorageService,
        private authService: AuthService) {
        this.route.params.subscribe((params: any) => {
            this.isAdvancedSearch = this.router.url === '/discover/advanced/search';
            if (this.isAdvancedSearch) {
                this.loadAdvancedData();
                return;
            }
            this.searchTerm = params.searchTerm;
            this.clear();
            this.init();
        });

        this.advancedDataService.onDataChange.subscribe(() => {
            this.clear();
            this.loadAdvancedData();
        });

    }

    public async ngOnInit() {
        this.isAdmin = this.authService.isAdmin();

        if (this.advancedDataService) {
            return;
        }
        this.loadingFlag = true;

        this.filterService.onFilterChange.subscribe(async x => {
            if (!isEqual(this.filter, x)) {
                this.filter = { ...x };
                await this.search();
            }
        });
    }

    public async init() {
        var filter = this.store.get("searchFilter");
        if (filter) {
            this.filter = Object.assign(new SearchFilter(), JSON.parse(filter));
        } else {
            this.filter = new SearchFilter({ movies: true, tvShows: true, people: false, music: false });
        }
        this.loading();
        await this.search();
    }

    public createInitalModel() {
        this.finishLoading();
        this.results.forEach(m => {

            let mediaType = RequestType.movie;
            if (m.mediaType == "movie") {
                mediaType = RequestType.movie;
            } else if (m.mediaType == "tv") {
                mediaType = RequestType.tvShow;
            } else if (m.mediaType == "Artist") {
                mediaType = RequestType.artist;
            } else if (m.mediaType == "Album") {
                mediaType = RequestType.album;
            }

            let poster = `https://image.tmdb.org/t/p/w300/${m.poster}`;
            if (!m.poster) {
                if (mediaType === RequestType.movie) {
                    poster = "images/default_movie_poster.png"
                }
                if (mediaType === RequestType.tvShow) {
                    poster = "images/default_tv_poster.png"
                }
            }

            this.discoverResults.push({
                posterPath: mediaType !== RequestType.artist && mediaType !== RequestType.album ? poster : "images/default-music-placeholder.png",
                requested: false,
                title: m.title,
                type: mediaType,
                id: m.id,
                url: "",
                rating: 0,
                overview: m.overview,
                approved: false,
                imdbid: "",
                denied: false,
                background: "",
                available: false,
                tvMovieDb: mediaType === RequestType.tvShow ? true : false
            });
        });
    }

    private loading() {
        this.loadingFlag = true;
    }

    private finishLoading() {
        this.loadingFlag = false;
    }

    private clear() {
        this.results = [];
        this.discoverResults = [];
    }

    private loadAdvancedData() {
        const advancedData = this.advancedDataService.getData();
        this.mapAdvancedData(advancedData);
        return;
    }

    public mapAdvancedData(advancedData: ISearchMovieResult[]) {
        this.finishLoading();
        const type = this.advancedDataService.getType();
        advancedData.forEach(m => {

            let mediaType = type;

            let poster = `https://image.tmdb.org/t/p/w300/${m.posterPath}`;
            if (!m.posterPath) {
                if (mediaType === RequestType.movie) {
                    poster = "images/default_movie_poster.png"
                }
                if (mediaType === RequestType.tvShow) {
                    poster = "images/default_tv_poster.png"
                }
            }

            this.discoverResults.push({
                posterPath:  poster,
                requested: false,
                title: m.title,
                type: mediaType,
                id: m.id,
                url: "",
                rating: 0,
                overview: m.overview,
                approved: false,
                imdbid: "",
                denied: false,
                background: "",
                available: false,
                tvMovieDb: false
            });
        });
    }

    private async search() {
        this.clear();
        this.results = await this.searchService
            .multiSearch(this.searchTerm, this.filter).toPromise();
        this.createInitalModel();
    }
}
