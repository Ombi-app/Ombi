import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { Injectable, Inject } from "@angular/core";

import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

import { IIssueCategory, IIssueComments, IIssueCount, IIssues, IIssuesChat, IIssuesSummary, INewIssueComments, IssueStatus, IUpdateStatus } from "../interfaces";
import { ServiceHelpers } from "./service.helpers";

@Injectable()
export class IssuesV2Service extends ServiceHelpers {
    constructor(http: HttpClient, @Inject(APP_BASE_HREF) href:string) {
        super(http, "/api/v2/Issues/", href);
    }

    public getIssues(position: number, take: number, status: IssueStatus): Observable<IIssuesSummary[]> {
        return this.http.get<IIssuesSummary[]>(`${this.url}${position}/${take}/${status}`,  {headers: this.headers});
    }

    public getIssuesByProviderId(providerId: string): Observable<IIssuesSummary> {
        return this.http.get<IIssuesSummary>(`${this.url}details/${providerId}`,  {headers: this.headers});
    }
}
