import { APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IMultiSearchResult, ISearchMovieResult, ISearchTvResult } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

import { ISearchMovieResultV2 } from "../interfaces/ISearchMovieResultV2";
import { ISearchTvResultV2, IMovieCollectionsViewModel, IActorCredits } from "../interfaces/ISearchTvResultV2";
import { IArtistSearchResult, IAlbumArt, IReleaseGroups } from "../interfaces/IMusicSearchResultV2";
import { SearchFilter } from "../my-nav/SearchFilter";
import { IMovieRatings, ITvRatings } from "../interfaces/IRatings";
import { IStreamingData } from "../interfaces/IStreams";

@Injectable()
export class SearchV2Service extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/search", href);
    }

    public multiSearch(searchTerm: string, filter: SearchFilter): Observable<IMultiSearchResult[]> {
        return this.http.post<IMultiSearchResult[]>(`${this.url}/multi/${searchTerm}`, filter);
    }
    public getFullMovieDetails(theMovieDbId: number): Observable<ISearchMovieResultV2> {
        return this.http.get<ISearchMovieResultV2>(`${this.url}/Movie/${theMovieDbId}`);
    }

    public getMovieByImdbId(imdbId: string): Observable<ISearchMovieResultV2> {
        return this.http.get<ISearchMovieResultV2>(`${this.url}/Movie/imdb/${imdbId}`);
    }

    public getFullMovieDetailsByRequestId(requestId: number): Promise<ISearchMovieResultV2> {
        return this.http.get<ISearchMovieResultV2>(`${this.url}/Movie/request/${requestId}`).toPromise();
    }

    public getFullMovieDetailsPromise(theMovieDbId: number): Promise<ISearchMovieResultV2> {
        return this.http.get<ISearchMovieResultV2>(`${this.url}/Movie/${theMovieDbId}`).toPromise();
    }

    public similarMovies(theMovieDbId: number, langCode: string): Observable<ISearchMovieResult[]> {
        return this.http.post<ISearchMovieResult[]>(`${this.url}/Movie/similar`, {theMovieDbId, languageCode: langCode});
    }

    public popularMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/Popular`);
    }

    public popularMoviesByPage(currentlyLoaded: number, toLoad: number): Promise<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/Popular/${currentlyLoaded}/${toLoad}`).toPromise();
    }

    public upcomingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/upcoming`);
    }

    public upcomingMoviesByPage(currentlyLoaded: number, toLoad: number): Promise<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/upcoming/${currentlyLoaded}/${toLoad}`).toPromise();
    }

    public nowPlayingMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/nowplaying`);
    }
    public nowPlayingMoviesByPage(currentlyLoaded: number, toLoad: number): Promise<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/nowplaying/${currentlyLoaded}/${toLoad}`).toPromise();
    }

    public topRatedMovies(): Observable<ISearchMovieResult[]> {
        return this.http.get<ISearchMovieResult[]>(`${this.url}/Movie/toprated`);
    }

    public popularTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/popular`, { headers: this.headers });
    }

    public popularTvByPage(currentlyLoaded: number, toLoad: number): Promise<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/popular/${currentlyLoaded}/${toLoad}`, { headers: this.headers }).toPromise();
    }

    public mostWatchedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/mostwatched`, { headers: this.headers });
    }
    public anticipatedTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/anticipated`, { headers: this.headers });
    }
    public anticipatedTvByPage(currentlyLoaded: number, toLoad: number): Promise<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/anticipated/${currentlyLoaded}/${toLoad}`, { headers: this.headers }).toPromise();
    }

    public trendingTv(): Observable<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/trending`, { headers: this.headers });
    }

    public trendingTvByPage(currentlyLoaded: number, toLoad: number): Promise<ISearchTvResult[]> {
        return this.http.get<ISearchTvResult[]>(`${this.url}/Tv/trending/${currentlyLoaded}/${toLoad}`, { headers: this.headers }).toPromise();
    }

    public getTvInfo(tvdbid: number): Promise<ISearchTvResultV2> {
        return this.http.get<ISearchTvResultV2>(`${this.url}/Tv/${tvdbid}`, { headers: this.headers }).toPromise();
    }

    public getTvInfoWithRequestId(requestId: number): Promise<ISearchTvResultV2> {
        return this.http.get<ISearchTvResultV2>(`${this.url}/Tv/request/${requestId}`, { headers: this.headers }).toPromise();
    }

    public getTvInfoWithMovieDbId(theMovieDbId: number): Promise<ISearchTvResultV2> {
        return this.http.get<ISearchTvResultV2>(`${this.url}/Tv/moviedb/${theMovieDbId}`, { headers: this.headers }).toPromise();
    }

    public getMovieCollections(collectionId: number): Promise<IMovieCollectionsViewModel> {
        return this.http.get<IMovieCollectionsViewModel>(`${this.url}/movie/collection/${collectionId}`, { headers: this.headers }).toPromise();
    }

    public getMoviesByActor(actorId: number): Observable<IActorCredits> {
        return this.http.get<IActorCredits>(`${this.url}/actor/${actorId}/movie`, { headers: this.headers });
    }

    public getArtistInformation(artistId: string): Observable<IArtistSearchResult> {
        return this.http.get<IArtistSearchResult>(`${this.url}/artist/${artistId}`);
    }

    public getReleaseGroupArt(mbid: string): Observable<IAlbumArt> {
        return this.http.get<IAlbumArt>(`${this.url}/releasegroupart/${mbid}`);
    }

    public getAlbum(mbid: string): Observable<IReleaseGroups> {
        return this.http.get<IReleaseGroups>(`${this.url}/artist/album/${mbid}`);
    }

    public getRottenMovieRatings(name: string, year: number): Observable<IMovieRatings> {
        return this.http.get<IMovieRatings>(`${this.url}/ratings/movie/${name}/${year}`);
    }

    public getRottenTvRatings(name: string, year: number): Observable<ITvRatings> {
        return this.http.get<ITvRatings>(`${this.url}/ratings/tv/${name}/${year}`);
    }

    public getMovieStreams(theMovieDbId: number): Observable<IStreamingData[]> {
        return this.http.get<IStreamingData[]>(`${this.url}/stream/movie/${theMovieDbId}`);
    }

    public getTvStreams(theTvDbId: number, tvMaze: number): Observable<IStreamingData[]> {
        return this.http.get<IStreamingData[]>(`${this.url}/stream/tv/${theTvDbId}/${tvMaze}`);
    }

}
