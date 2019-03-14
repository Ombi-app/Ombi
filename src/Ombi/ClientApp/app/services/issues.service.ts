import { PlatformLocation } from "@angular/common";
import { Injectable } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IIssueCategory, IIssueComments, IIssueCount, IIssues, IIssuesChat, INewIssueComments, IssueStatus, IUpdateStatus } from "../interfaces";
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

    public getIssues(): Observable<IIssues[]> {
        return this.http.get<IIssues[]>(this.url,  {headers: this.headers});
    }

    public getIssuesPage(take: number, skip: number, status: IssueStatus): Observable<IIssues[]> {
        return this.http.get<IIssues[]>(`${this.url}${take}/${skip}/${status}`,  {headers: this.headers});
    }

    public getIssuesCount(): Observable<IIssueCount> {
        return this.http.get<IIssueCount>(`${this.url}count`,  {headers: this.headers});
    }

    public createIssue(issue: IIssues): Observable<number> {
        return this.http.post<number>(this.url, JSON.stringify(issue), {headers: this.headers});
    }

    public getIssue(id: number): Observable<IIssues> {
        return this.http.get<IIssues>(`${this.url}${id}`,  {headers: this.headers});
    }

    public getComments(id: number): Observable<IIssuesChat[]> {
        return this.http.get<IIssuesChat[]>(`${this.url}${id}/comments`,  {headers: this.headers});
    }

    public addComment(comment: INewIssueComments): Observable<IIssueComments> {
        return this.http.post<IIssueComments>(`${this.url}comments`, JSON.stringify(comment),  {headers: this.headers});
    }

    public updateStatus(model: IUpdateStatus): Observable<boolean> {
        return this.http.post<boolean>(`${this.url}status`, JSON.stringify(model), { headers: this.headers });
    }

    public deleteComment(id: number): Observable<boolean> {
        return this.http.delete<boolean>(`${this.url}comments/${id}`, { headers: this.headers });
    }
}
