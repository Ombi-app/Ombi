import { APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ServiceHelpers } from "./service.helpers";
import { IRequestsViewModel, IMovieRequests, IChildRequests, IMovieAdvancedOptions as IMediaAdvancedOptions, IRequestEngineResult, IAlbumRequest } from "../interfaces";


@Injectable()
export class RequestServiceV2 extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/Requests/", href);
    }

    public getMovieRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getMovieAvailableRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/available/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getMovieProcessingRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/processing/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getMoviePendingRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/pending/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getMovieDeniedRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/denied/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getTvRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IChildRequests>> {
        return this.http.get<IRequestsViewModel<IChildRequests>>(`${this.url}tv/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getPendingTvRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IChildRequests>> {
        return this.http.get<IRequestsViewModel<IChildRequests>>(`${this.url}tv/pending/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getProcessingTvRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IChildRequests>> {
        return this.http.get<IRequestsViewModel<IChildRequests>>(`${this.url}tv/processing/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getAvailableTvRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IChildRequests>> {
        return this.http.get<IRequestsViewModel<IChildRequests>>(`${this.url}tv/available/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getDeniedTvRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IChildRequests>> {
        return this.http.get<IRequestsViewModel<IChildRequests>>(`${this.url}tv/denied/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public updateMovieAdvancedOptions(options: IMediaAdvancedOptions): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}movie/advancedoptions`, options, {headers: this.headers});
    }

    public updateTvAdvancedOptions(options: IMediaAdvancedOptions): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}tv/advancedoptions`, options, {headers: this.headers});
    }

    public getMovieUnavailableRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/unavailable/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getTvUnavailableRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IChildRequests>> {
        return this.http.get<IRequestsViewModel<IChildRequests>>(`${this.url}tv/unavailable/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getAlbumRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IAlbumRequest>> {
        return this.http.get<IRequestsViewModel<IAlbumRequest>>(`${this.url}Album/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getAlbumAvailableRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IAlbumRequest>> {
        return this.http.get<IRequestsViewModel<IAlbumRequest>>(`${this.url}Album/available/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getAlbumProcessingRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IAlbumRequest>> {
        return this.http.get<IRequestsViewModel<IAlbumRequest>>(`${this.url}Album/processing/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getAlbumPendingRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IAlbumRequest>> {
        return this.http.get<IRequestsViewModel<IAlbumRequest>>(`${this.url}Album/pending/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }

    public getAlbumDeniedRequests(count: number, position: number, sortProperty: string , order: string): Observable<IRequestsViewModel<IAlbumRequest>> {
        return this.http.get<IRequestsViewModel<IAlbumRequest>>(`${this.url}Album/denied/${count}/${position}/${sortProperty}/${order}`, {headers: this.headers});
    }
}
