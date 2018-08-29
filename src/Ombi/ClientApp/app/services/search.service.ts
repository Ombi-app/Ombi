import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { TreeNode } from "primeng/primeng";
import { ISearchMovieResult } from "../interfaces";
import { ISearchTvResult } from "../interfaces";
import { ISearchAlbumResult, ISearchArtistResult } from "../interfaces/ISearchMusicResult";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class SearchService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/search", platformLocation);
    }

    // Movies
    public searchMovie(searchTerm: string): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/` + searchTerm);
    }
    public similarMovies(theMovieDbId: number): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/${theMovieDbId}/similar`);
    }

    public popularMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/Popular`);
    }
    public upcomingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/upcoming`);
    }
    public nowPlayingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/nowplaying`);
    }
    public topRatedMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/toprated`);
    }
    public getMovieInformation(theMovieDbId: number): Observable<ISearchMovieResult> {
        return this.http.get<ISearchMovieResult>(`${this.url}/Movie/info/${theMovieDbId}`);
    }

    // TV
    public searchTv(searchTerm: string): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/${searchTerm}`, {headers: this.headers});
    }

    public searchTvTreeNode(searchTerm: string): Observable<TreeNode[]> {
        return this.http.get<TreeNode[]>(`${this.url}/Tv/${searchTerm}/tree`, {headers: this.headers});
    }

    public getShowInformationTreeNode(theTvDbId: number): Observable<TreeNode> {
        return this.http.get<TreeNode>(`${this.url}/Tv/info/${theTvDbId}/Tree`, {headers: this.headers});
    }

    public getShowInformation(theTvDbId: number): Observable<ISearchTvResult> {
        return this.http.get<ISearchTvResult>(`${this.url}/Tv/info/${theTvDbId}`, {headers: this.headers});
    }

    public popularTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/popular`, {headers: this.headers});
    }
    public mostWatchedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/mostwatched`, {headers: this.headers});
    }
    public anticipatedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/anticipated`, {headers: this.headers});
    }
    public trendingTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/trending`, {headers: this.headers});
    }
    // Music
    public searchArtist(searchTerm: string): Observable<ISearchArtistResult[]> {
        return this.http.get<ISearchArtistResult[]>(`${this.url}/Music/Artist/` + searchTerm);
    }
    public searchAlbum(searchTerm: string): Observable<ISearchAlbumResult[]> {
        return this.http.get<ISearchAlbumResult[]>(`${this.url}/Music/Album/` + searchTerm);
    }
    public getAlbumsForArtist(foreignArtistId: string): Observable<ISearchAlbumResult[]> {
        return this.http.get<ISearchAlbumResult[]>(`${this.url}/Music/Artist/Album/${foreignArtistId}`);
    }
}
