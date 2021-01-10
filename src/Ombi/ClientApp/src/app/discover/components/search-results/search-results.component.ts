import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { SearchV2Service } from "../../../services";
import { IDiscoverCardResult } from "../../interfaces";
import { IMultiSearchResult, RequestType } from "../../../interfaces";
import { FilterService } from "../../services/filter-service";
import { SearchFilter } from "../../../my-nav/SearchFilter";
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

    public discoverResults: IDiscoverCardResult[] = [];

    public filter: SearchFilter;

    constructor(private searchService: SearchV2Service,
        private route: ActivatedRoute,
        private filterService: FilterService,
        private store: StorageService) {
        this.route.params.subscribe((params: any) => {
            this.searchTerm = params.searchTerm;
            this.clear();
            this.init();
        });
    }

    public async ngOnInit() {
        this.loadingFlag = true;

        this.filterService.onFilterChange.subscribe(async x => {
            if (!isEqual(this.filter, x)) {
                this.filter =  { ...x };
                await this.search();
            }
        });
    }

    public async init() {
        var filter = this.store.get("searchFilter");
        if (filter) {
            this.filter = Object.assign(new SearchFilter(), JSON.parse(filter));
        } else {
            this.filter = new SearchFilter({ movies: true, tvShows: true, people: false, music: false});
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
                mediaType = RequestType.album;
            }

            this.discoverResults.push({
                posterPath: `https://image.tmdb.org/t/p/w300/${m.poster}`,
                requested: false,
                title: m.title,
                type: mediaType,
                id: +m.id,
                url: "",
                rating: 0,
                overview: "",
                approved: false,
                imdbid: "",
                denied: false,
                background: "",
                available: false,
                tvMovieDb: mediaType === RequestType.tvShow ? true : false
            });

            // switch (mediaType) {
            //     case RequestType.movie:
            //         this.searchService.getFullMovieDetails(+m.id)
            //         .subscribe(x => {
            //             const index = this.discoverResults.findIndex((obj => obj.id === +m.id));
            //             this.discoverResults[index].available = x.available;
            //             this.discoverResults[index].requested = x.requested;
            //             this.discoverResults[index].requested = x.requested;
            //             this.discoverResults[index].requested = x.requested;
            //             this.discoverResults[index].requested = x.requested;
            //             this.discoverResults[index].requested = x.requested;
            //         });
            // }
        });
    }

    // private createModel() {
    //     this.finishLoading();
    //     this.collection.collection.forEach(m => {
    //         this.discoverResults.push({
    //             available: m.available,
    //             posterPath: `https://image.tmdb.org/t/p/w300/${m.posterPath}`,
    //             requested: m.requested,
    //             title: m.title,
    //             type: RequestType.movie,
    //             id: m.id,
    //             url: `http://www.imdb.com/title/${m.imdbId}/`,
    //             rating: 0,
    //             overview: m.overview,
    //             approved: m.approved,
    //             imdbid: m.imdbId,
    //             denied:false,
    //             background: ""
    //         });
    //     });
    // }

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

    private async search() {
        this.clear();
        this.results = await this.searchService
            .multiSearch(this.searchTerm, this.filter).toPromise();
        this.createInitalModel();
    }
}
