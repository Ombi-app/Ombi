import { Component, OnInit } from "@angular/core";

import { IssuesService } from "../services";

import { IIssueCount, IIssues, IPagenator, IssueStatus } from "../interfaces";

import { PageEvent } from '@angular/material/paginator';

@Component({
    templateUrl: "issues.component.html",
    styleUrls: ['issues.component.scss']
})
export class IssuesComponent implements OnInit {

    public pendingIssues: IIssues[];
    public inProgressIssues: IIssues[];
    public resolvedIssues: IIssues[];

    public count: IIssueCount;

    private takeAmount = 50;
    private pendingSkip = 0;
    private inProgressSkip = 0;
    private resolvedSkip = 0;

    constructor(private issueService: IssuesService) { }

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
        this.issueService.getIssuesPage(this.takeAmount, this.pendingSkip, IssueStatus.Pending).subscribe(x => {
            this.pendingIssues = x;
        });
    }

    private getInProg() {
        this.issueService.getIssuesPage(this.takeAmount, this.inProgressSkip, IssueStatus.InProgress).subscribe(x => {
            this.inProgressIssues = x;
        });
    }

    private getResolved() {
        this.issueService.getIssuesPage(this.takeAmount, this.resolvedSkip, IssueStatus.Resolved).subscribe(x => {
            this.resolvedIssues = x;
        });
    }
}
