import { Component, OnInit } from "@angular/core";

import { IssuesService } from "../services";

import { IIssueCount, IIssues, IIssuesSummary, IPagenator, IssueStatus } from "../interfaces";

import { PageEvent } from '@angular/material/paginator';
import { IssuesV2Service } from "../services/issuesv2.service";

@Component({
    templateUrl: "issues.component.html",
    styleUrls: ['issues.component.scss']
})
export class IssuesComponent implements OnInit {

    public pendingIssues: IIssuesSummary[];
    public inProgressIssues: IIssuesSummary[];
    public resolvedIssues: IIssuesSummary[];

    public count: IIssueCount;

    private takeAmount = 50;
    private pendingSkip = 0;
    private inProgressSkip = 0;
    private resolvedSkip = 0;

    constructor(private issuev2Service: IssuesV2Service, private issueService: IssuesService) { }

    public ngOnInit() {
        this.getPending();
        this.getInProg();
        this.getResolved();
        this.issueService.getIssuesCount().subscribe(x => this.count = x);
    }

    public changePagePending(event: PageEvent) {
        this.pendingSkip = event.pageSize * event.pageIndex++;
        this.getPending();
    }

    public changePageInProg(event: PageEvent) {
        this.inProgressSkip = event.pageSize * event.pageIndex++;
        this.getInProg();
    }

    public changePageResolved(event: PageEvent) {
        this.resolvedSkip = event.pageSize * event.pageIndex++;
        this.getResolved();
    }

    private getPending() {
        this.issuev2Service.getIssues(this.pendingSkip, this.takeAmount, IssueStatus.Pending).subscribe(x => {
            this.pendingIssues = x;
        });
    }

    private getInProg() {
        this.issuev2Service.getIssues(this.inProgressSkip, this.takeAmount, IssueStatus.InProgress).subscribe(x => {
            this.inProgressIssues = x;
        });
    }

    private getResolved() {
        this.issuev2Service.getIssues(this.resolvedSkip, this.takeAmount, IssueStatus.Resolved).subscribe(x => {
            this.resolvedIssues = x;
        });
    }
}
