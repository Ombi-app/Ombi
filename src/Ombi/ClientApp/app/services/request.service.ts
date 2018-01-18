import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Rx";

import { TreeNode } from "primeng/primeng";
import { IRequestEngineResult } from "../interfaces";
import { IChildRequests, IMovieRequestModel, IMovieRequests, IMovieUpdateModel, ITvRequests, ITvUpdateModel } from "../interfaces";
import { ISearchTvResult } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class RequestService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Request/", platformLocation);
    }

    public requestMovie(movie: IMovieRequestModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}Movie/`, JSON.stringify(movie),  {headers: this.headers});
    }

    public requestTv(tv: ISearchTvResult): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}TV/`, JSON.stringify(tv), {headers: this.headers});
    }

    public approveMovie(movie: IMovieUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}Movie/Approve`, JSON.stringify(movie),  {headers: this.headers});
    }

    public denyMovie(movie: IMovieUpdateModel): Observable<IRequestEngineResult> {
        return this.http.put<IRequestEngineResult>(`${this.url}Movie/Deny`, JSON.stringify(movie),  {headers: this.headers});
    }

    public markMovieAvailable(movie: IMovieUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}Movie/available`, JSON.stringify(movie),  {headers: this.headers});
    }

    public markMovieUnavailable(movie: IMovieUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}Movie/unavailable`, JSON.stringify(movie),  {headers: this.headers});
    }

    public getMovieRequests(count: number, position: number): Observable<IMovieRequests[]> {
        return this.http.get<IMovieRequests[]>(`${this.url}movie/${count}/${position}`, {headers: this.headers});
    }

    public searchMovieRequests(search: string): Observable<IMovieRequests[]> {
        return this.http.get<IMovieRequests[]>(`${this.url}movie/search/${search}`, {headers: this.headers});
    }

    public removeMovieRequest(request: IMovieRequests) {
        this.http.delete(`${this.url}movie/${request.id}`, {headers: this.headers}).subscribe();
    }

    public updateMovieRequest(request: IMovieRequests): Observable<IMovieRequests> {
        return this.http.put<IMovieRequests>(`${this.url}movie/`, JSON.stringify(request), {headers: this.headers});
    }

    public getTvRequests(count: number, position: number): Observable<ITvRequests[]> {
        return this.http.get<ITvRequests[]>(`${this.url}tv/${count}/${position}`, {headers: this.headers});
    }

    public getTvRequestsTree(count: number, position: number): Observable<TreeNode[]> {
        return this.http.get<TreeNode[]>(`${this.url}tv/${count}/${position}/tree`, {headers: this.headers});
    }

     public getChildRequests(requestId: number): Observable<IChildRequests[]> {
        return this.http.get<IChildRequests[]>(`${this.url}tv/${requestId}/child`, {headers: this.headers});
    }

    public searchTvRequests(search: string): Observable<ITvRequests[]> {
        return this.http.get<ITvRequests[]>(`${this.url}tv/search/${search}`, {headers: this.headers});
     }

    public searchTvRequestsTree(search: string): Observable<TreeNode[]> {
        return this.http.get<TreeNode[]>(`${this.url}tv/search/${search}/tree`, {headers: this.headers});
    }

    public removeTvRequest(request: ITvRequests) {
        this.http.delete(`${this.url}tv/${request.id}`, {headers: this.headers}).subscribe();
    }
    
    public markTvAvailable(movie: ITvUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}tv/available`, JSON.stringify(movie),  {headers: this.headers});
    }

    public markTvUnavailable(movie: ITvUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}tv/unavailable`, JSON.stringify(movie),  {headers: this.headers});
    }

    public updateTvRequest(request: ITvRequests): Observable<ITvRequests> {
        return this.http.put<ITvRequests>(`${this.url}tv/`, JSON.stringify(request), {headers: this.headers});
    }

    public updateChild(child: IChildRequests): Observable<IChildRequests> {
        return this.http.put<IChildRequests>(`${this.url}tv/child`, JSON.stringify(child), {headers: this.headers});
    }   

    public denyChild(child: ITvUpdateModel): Observable<IRequestEngineResult> {
        return this.http.put<IRequestEngineResult>(`${this.url}tv/deny`, JSON.stringify(child), {headers: this.headers});
    } 

    public approveChild(child: ITvUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}tv/approve`, JSON.stringify(child), {headers: this.headers});
    }
    public deleteChild(child: IChildRequests): Observable<boolean> {
        return this.http.delete<boolean>(`${this.url}tv/child/${child.id}`, {headers: this.headers});
    }
}
