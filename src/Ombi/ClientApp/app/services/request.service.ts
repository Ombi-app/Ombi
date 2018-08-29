import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { TreeNode } from "primeng/primeng";
import { FilterType, IAlbumRequest, IAlbumRequestModel, IAlbumUpdateModel, IChildRequests, IFilter, IMovieRequestModel, IMovieRequests,
      IMovieUpdateModel, IRequestEngineResult, IRequestsViewModel, ITvRequests, ITvUpdateModel, OrderType } from "../interfaces";
import { ITvRequestViewModel } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class RequestService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Request/", platformLocation);
    }

    public requestMovie(movie: IMovieRequestModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}Movie/`, JSON.stringify(movie),  {headers: this.headers});
    }

    public getTotalMovies(): Observable<number> {
        return this.http.get<number>(`${this.url}Movie/total`, {headers: this.headers});
    }

    public getTotalTv(): Observable<number> {
        return this.http.get<number>(`${this.url}tv/total`, {headers: this.headers});
    }

    public requestTv(tv: ITvRequestViewModel): Observable<IRequestEngineResult> {
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

    public getMovieRequests(count: number, position: number, order: OrderType, filter: IFilter): Observable<IRequestsViewModel<IMovieRequests>> {
        return this.http.get<IRequestsViewModel<IMovieRequests>>(`${this.url}movie/${count}/${position}/${order}/${filter.statusFilter}/${filter.availabilityFilter}`, {headers: this.headers});
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

    public getTvRequests(count: number, position: number, order: OrderType, status: FilterType, availability: FilterType): Observable<IRequestsViewModel<ITvRequests>> {
        return this.http.get<IRequestsViewModel<ITvRequests>>(`${this.url}tv/${count}/${position}/${order}/${status}/${availability}`, {headers: this.headers});
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

    public subscribeToMovie(requestId: number): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}movie/subscribe/${requestId}`, {headers: this.headers});
    }
    public unSubscribeToMovie(requestId: number): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}movie/unsubscribe/${requestId}`, {headers: this.headers});
    }
    public subscribeToTv(requestId: number): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}tv/subscribe/${requestId}`, {headers: this.headers});
    }
    public unSubscribeToTv(requestId: number): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}tv/unsubscribe/${requestId}`, {headers: this.headers});
    }
    public setQualityProfile(requestId: number, qualityId: number): Observable<boolean> {
        return this.http.put<boolean>(`${this.url}tv/quality/${requestId}/${qualityId}`, {headers: this.headers});
    }
    public setRootFolder(requestId: number, rootFolderId: number): Observable<boolean> {
        return this.http.put<boolean>(`${this.url}tv/root/${requestId}/${rootFolderId}`, {headers: this.headers});
    }

    // Music
    public requestAlbum(Album: IAlbumRequestModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}music/`, JSON.stringify(Album),  {headers: this.headers});
    }

    public getTotalAlbums(): Observable<number> {
        return this.http.get<number>(`${this.url}music/total`, {headers: this.headers});
    }

    public approveAlbum(Album: IAlbumUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}music/Approve`, JSON.stringify(Album),  {headers: this.headers});
    }

    public denyAlbum(Album: IAlbumUpdateModel): Observable<IRequestEngineResult> {
        return this.http.put<IRequestEngineResult>(`${this.url}music/Deny`, JSON.stringify(Album),  {headers: this.headers});
    }

    public markAlbumAvailable(Album: IAlbumUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}music/available`, JSON.stringify(Album),  {headers: this.headers});
    }

    public markAlbumUnavailable(Album: IAlbumUpdateModel): Observable<IRequestEngineResult> {
        return this.http.post<IRequestEngineResult>(`${this.url}music/unavailable`, JSON.stringify(Album),  {headers: this.headers});
    }

    public getAlbumRequests(count: number, position: number, order: OrderType, filter: IFilter): Observable<IRequestsViewModel<IAlbumRequest>> {
        return this.http.get<IRequestsViewModel<IAlbumRequest>>(`${this.url}music/${count}/${position}/${order}/${filter.statusFilter}/${filter.availabilityFilter}`, {headers: this.headers});
    }

    public searchAlbumRequests(search: string): Observable<IAlbumRequest[]> {
        return this.http.get<IAlbumRequest[]>(`${this.url}music/search/${search}`, {headers: this.headers});
    }

    public removeAlbumRequest(request: IAlbumRequest): any {
        this.http.delete(`${this.url}music/${request.id}`, {headers: this.headers}).subscribe();
    }

}
