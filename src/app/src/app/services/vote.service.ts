import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";

import { IVoteEngineResult, IVoteViewModel } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class VoteService extends ServiceHelpers {
    constructor(public http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v1/Vote/", href);
    }

    public async getModel(): Promise<IVoteViewModel[]> {
        return await this.http.get<IVoteViewModel[]>(`${this.url}`, {headers: this.headers}).toPromise();
    }

    public async upvoteMovie(requestId: number): Promise<IVoteEngineResult> {
        return await this.http.post<IVoteEngineResult>(`${this.url}up/movie/${requestId}`, {headers: this.headers}).toPromise();
    }
    public async upvoteTv(requestId: number): Promise<IVoteEngineResult> {
        return await this.http.post<IVoteEngineResult>(`${this.url}up/tv/${requestId}`, {headers: this.headers}).toPromise();
    }
    public async upvoteAlbum(requestId: number): Promise<IVoteEngineResult> {
        return await this.http.post<IVoteEngineResult>(`${this.url}up/album/${requestId}`, {headers: this.headers}).toPromise();
    }
    public async downvoteMovie(requestId: number): Promise<IVoteEngineResult> {
        return await this.http.post<IVoteEngineResult>(`${this.url}down/movie/${requestId}`, {headers: this.headers}).toPromise();
    }
    public async downvoteTv(requestId: number): Promise<IVoteEngineResult> {
        return await this.http.post<IVoteEngineResult>(`${this.url}down/tv/${requestId}`, {headers: this.headers}).toPromise();
    }
    public async downvoteAlbum(requestId: number): Promise<IVoteEngineResult> {
        return await this.http.post<IVoteEngineResult>(`${this.url}down/album/${requestId}`, {headers: this.headers}).toPromise();
    }
}
