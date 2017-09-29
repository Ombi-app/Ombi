import { Injectable } from "@angular/core";
import { PlatformLocation } from "@angular/common";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { TreeNode } from "primeng/primeng";
import { ISearchMovieResult } from "../interfaces";
import { ISearchTvResult } from "../interfaces";
import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class SearchService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/search", platformLocation);
    }

    // Movies
    public searchMovie(searchTerm: string): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/` + searchTerm).map(this.extractData);
    }

    public popularMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/Popular`).map(this.extractData);
    }
    public upcomignMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/upcoming`).map(this.extractData);
    }
    public nowPlayingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/nowplaying`).map(this.extractData);
    }
    public topRatedMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get(`${this.url}/Movie/toprated`).map(this.extractData);
    }
    public getMovieInformation(theMovieDbId: number): Observable<ISearchMovieResult> {
        return this.http.get(`${this.url}/Movie/info/${theMovieDbId}`).map(this.extractData);
    }

    // TV
    public searchTv(searchTerm: string): Observable<ISearchTvResult[]> {
        return this.http.get(`${this.url}/Tv/` + searchTerm).map(this.extractData);
    }

    public searchTvTreeNode(searchTerm: string): Observable<TreeNode[]> {
        return this.http.get(`${this.url}/Tv/${searchTerm}/tree`).map(this.extractData);
    }

    public getShowInformationTreeNode(theTvDbId: number): Observable<TreeNode> {
        return this.http.get(`${this.url}/Tv/info/${theTvDbId}/Tree`).map(this.extractData);
    }

    public getShowInformation(theTvDbId: number): Observable<ISearchTvResult> {
        return this.http.get(`${this.url}/Tv/info/${theTvDbId}`).map(this.extractData);
    }

    public popularTv(): Observable<TreeNode[]> {
        return this.http.get(`${this.url}/Tv/popular`).map(this.extractData);
    }
    public mostWatchedTv(): Observable<TreeNode[]> {
        return this.http.get(`${this.url}/Tv/mostwatched`).map(this.extractData);
    }
    public anticipatedTv(): Observable<TreeNode[]> {
        return this.http.get(`${this.url}/Tv/anticipated`).map(this.extractData);
    }
    public trendingTv(): Observable<TreeNode[]> {
        return this.http.get(`${this.url}/Tv/trending`).map(this.extractData);
    }
}
