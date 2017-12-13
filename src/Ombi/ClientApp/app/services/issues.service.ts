import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Rx";

import { IIssueCategory, IIssueComments, IIssuesChat, IMovieIssues, INewIssueComments,ITvIssues } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class IssuesService extends ServiceHelpers {
    constructor(http: HttpClient, public platformLocation: PlatformLocation) {
        super(http, "/api/v1/Issues/", platformLocation);
    }

    public getCategories(): Observable<IIssueCategory[]> {
        return this.http.get<IIssueCategory[]>(`${this.url}categories/`,  {headers: this.headers});
    }

    public createCategory(cat: IIssueCategory): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}categories/`, JSON.stringify(cat), {headers: this.headers});
    }

    public deleteCategory(cat: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.url}categories/${cat}`, {headers: this.headers});
    }

    public getMovieIssues(): Observable<IMovieIssues[]> {
        return this.http.get<IMovieIssues[]>(`${this.url}movie/`,  {headers: this.headers});
    }

    public getTvIssues(): Observable<ITvIssues[]> {
        return this.http.get<ITvIssues[]>(`${this.url}tv/`,  {headers: this.headers});
    }

    public createMovieIssue(issue: IMovieIssues): Observable<number> {
        return this.http.post<number>(`${this.url}movie/`, JSON.stringify(issue), {headers: this.headers});
    }

    public createTvIssue(issue: ITvIssues): Observable<number> {
        return this.http.post<number>(`${this.url}tv/`, JSON.stringify(issue), {headers: this.headers});
    }
    
    public getTvIssue(id: number): Observable<ITvIssues> {
        return this.http.get<ITvIssues>(`${this.url}tv/${id}`,  {headers: this.headers});
    }    

    public getMovieIssue(id: number): Observable<IMovieIssues> {
        return this.http.get<IMovieIssues>(`${this.url}movie/${id}`,  {headers: this.headers});
    }

    public getMovieComments(id: number): Observable<IIssuesChat[]> {
        return this.http.get<IIssuesChat[]>(`${this.url}movie/${id}/comments`,  {headers: this.headers});
    }

    public getTvComments(id: number): Observable<IIssuesChat[]> {
        return this.http.get<IIssuesChat[]>(`${this.url}tv/${id}/comments`,  {headers: this.headers});
    }

    public addComment(comment: INewIssueComments): Observable<IIssueComments> {
        return this.http.post<IIssueComments>(`${this.url}comments`, JSON.stringify(comment),  {headers: this.headers});
    }
}
