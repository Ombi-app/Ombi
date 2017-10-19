import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";
import { Http } from "@angular/http";
import { AuthHttp } from "angular2-jwt";
import { Observable } from "rxjs/Rx";

import { TreeNode } from "primeng/primeng";
import { IRequestEngineResult } from "../interfaces";
import { IChildRequests, IMovieRequests, IMovieUpdateModel, IRequestCountModel, IRequestGrid, ITvRequests } from "../interfaces";
import { ISearchMovieResult } from "../interfaces";
import { ISearchTvResult } from "../interfaces";
import { ServiceAuthHelpers } from "./service.helpers";

@Injectable()
export class RequestService extends ServiceAuthHelpers {
    constructor(http: AuthHttp, private basicHttp: Http, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Request/", platformLocation);
    }

    public requestMovie(movie: ISearchMovieResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}Movie/`, JSON.stringify(movie), { headers: this.headers }).map(this.extractData);
    }

    public requestTv(tv: ISearchTvResult): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}TV/`, JSON.stringify(tv), { headers: this.headers }).map(this.extractData);
    }

    public approveMovie(movie: IMovieUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}Movie/Approve`, JSON.stringify(movie), { headers: this.headers }).map(this.extractData);
    }

    public getMovieRequests(count: number, position: number): Observable<IMovieRequests[]> {
        return this.http.get(`${this.url}movie/${count}/${position}`).map(this.extractData);
    }

    public searchMovieRequests(search: string): Observable<IMovieRequests[]> {
        return this.http.get(`${this.url}movie/search/${search}`).map(this.extractData);
    }

    public removeMovieRequest(request: IMovieRequests) {
        this.http.delete(`${this.url}movie/${request.id}`).map(this.extractData).subscribe();
    }

    public updateMovieRequest(request: IMovieRequests): Observable<IMovieRequests> {
        return this.http.put(`${this.url}movie/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }

    public getTvRequests(count: number, position: number): Observable<ITvRequests[]> {
        return this.http.get(`${this.url}tv/${count}/${position}`).map(this.extractData)
            .catch(this.handleError);
    }

    public getTvRequestsTree(count: number, position: number): Observable<TreeNode[]> {
        return this.http.get(`${this.url}tv/${count}/${position}/tree`).map(this.extractData)
            .catch(this.handleError);
    }

     public getChildRequests(requestId: number): Observable<IChildRequests[]> {
        return this.http.get(`${this.url}tv/${requestId}/child`).map(this.extractData)
            .catch(this.handleError);
    }

    public searchTvRequests(search: string): Observable<ITvRequests[]> {
        return this.http.get(`${this.url}tv/search/${search}`).map(this.extractData);
     }

    public searchTvRequestsTree(search: string): Observable<TreeNode[]> {
        return this.http.get(`${this.url}tv/search/${search}/tree`).map(this.extractData);
    }

    public removeTvRequest(request: ITvRequests) {
        this.http.delete(`${this.url}tv/${request.id}`).map(this.extractData).subscribe();
    }

    public updateTvRequest(request: ITvRequests): Observable<ITvRequests> {
        return this.http.put(`${this.url}tv/`, JSON.stringify(request), { headers: this.headers }).map(this.extractData);
    }
    public updateChild(child: IChildRequests): Observable<IChildRequests> {
        return this.http.put(`${this.url}tv/child`, JSON.stringify(child), { headers: this.headers }).map(this.extractData);
    }   
    public denyChild(child: IChildRequests): Observable<IChildRequests> {
        return this.http.put(`${this.url}tv/deny`, JSON.stringify(child), { headers: this.headers }).map(this.extractData);
    } 
    public changeAvailabilityChild(child: IChildRequests): Observable<IChildRequests> {
        return this.http.put(`${this.url}tv/changeavailability`, JSON.stringify(child), { headers: this.headers }).map(this.extractData);
    }   
    public approveChild(child: IChildRequests): Observable<IRequestEngineResult> {
        return this.http.post(`${this.url}tv/child/approve`, JSON.stringify(child), { headers: this.headers }).map(this.extractData);
    }
    public deleteChild(child: IChildRequests): Observable<IChildRequests> {
        return this.http.delete(`${this.url}tv/child/${child.id}`, { headers: this.headers }).map(this.extractData);
    }

    public getRequestsCount(): Observable<IRequestCountModel> {
        return this.basicHttp.get(`${this.url}count`).map(this.extractData);
    }

    public getMovieGrid(): Observable<IRequestGrid<IMovieRequests>> {
        return this.http.get(`${this.url}movie/grid`).map(this.extractData);
    }

    public getTvGrid(): Observable<IRequestGrid<ITvRequests>> {
        return this.http.get(`${this.url}tv/grid`).map(this.extractData);
    }
}
