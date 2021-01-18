import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { UITreeNode } from "primeng/tree";
import { ISearchMovieResult } from "../interfaces";
import { ISearchTvResult } from "../interfaces";
import { ISearchAlbumResult, ISearchArtistResult } from "../interfaces/ISearchMusicResult";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class SearchService extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/search", href);
    }

    // Movies
    public searchMovie(searchTerm: string): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/${searchTerm}`);
    }

    public searchMovieWithRefined(searchTerm: string, year: number | undefined, langCode: string): Observable<ISearchMovieResult[]> {
        return this.http.post<ISearchMovieResult[]>(`${this.url}/Movie/`, { searchTerm, year, languageCode: langCode });
    }

    public similarMovies(theMovieDbId: number, langCode: string): Observable<ISearchMovieResult[]> {
        return this.http.post<ISearchMovieResult[]>(`${this.url}/Movie/similar`, {theMovieDbId, languageCode: langCode});
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

    public getMovieInformationWithRefined(theMovieDbId: number, langCode: string): Observable<ISearchMovieResult> {
        return this.http.post<ISearchMovieResult>(`${this.url}/Movie/info`, { theMovieDbId, languageCode: langCode });
    }

    public searchMovieByActor(searchTerm: string, langCode: string): Observable<ISearchMovieResult[]> {
        return this.http.post<ISearchMovieResult[]>(`${this.url}/Movie/Actor`, { searchTerm, languageCode: langCode });
    }

    // TV
    public searchTv(searchTerm: string): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/${searchTerm}`, { headers: this.headers });
    }

    public searchTvTreeNode(searchTerm: string): Observable<UITreeNode[]> {
        return this.http.get<UITreeNode[]>(`${this.url}/Tv/${searchTerm}/tree`, { headers: this.headers });
    }

    public getShowInformationTreeNode(theTvDbId: number): Observable<UITreeNode> {
        return this.http.get<UITreeNode>(`${this.url}/Tv/info/${theTvDbId}/Tree`, { headers: this.headers });
    }

    public getShowInformation(theTvDbId: number): Observable<ISearchTvResult> {
        return this.http.get<ISearchTvResult>(`${this.url}/Tv/info/${theTvDbId}`, { headers: this.headers });
    }

    public popularTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/popular`, { headers: this.headers });
    }
    public mostWatchedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/mostwatched`, { headers: this.headers });
    }
    public anticipatedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/anticipated`, { headers: this.headers });
    }
    public trendingTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/trending`, { headers: this.headers });
    }
    // Music
    public searchArtist(searchTerm: string): Observable<ISearchArtistResult[]> {
        return this.http.get<ISearchArtistResult[]>(`${this.url}/Music/Artist/` + searchTerm);
    }
    public searchAlbum(searchTerm: string): Observable<ISearchAlbumResult[]> {
        return this.http.get<ISearchAlbumResult[]>(`${this.url}/Music/Album/` + searchTerm);
    }
    public getAlbumInformation(foreignAlbumId: string): Observable<ISearchAlbumResult> {
        return this.http.get<ISearchAlbumResult>(`${this.url}/Music/Album/info/` + foreignAlbumId);
    }
    public getAlbumsForArtist(foreignArtistId: string): Observable<ISearchAlbumResult[]> {
        return this.http.get<ISearchAlbumResult[]>(`${this.url}/Music/Artist/Album/${foreignArtistId}`);
    }
}
