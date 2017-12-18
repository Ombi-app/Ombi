import { Component, OnInit } from "@angular/core";

import { IssuesService } from "../services";

import { IIssues, IssueStatus } from "../interfaces";

@Component({
    templateUrl: "issues.component.html",
})
export class IssuesComponent implements OnInit {

    public pendingIssues: IIssues[];
    public inProgressIssues: IIssues[];
    public resolvedIssues: IIssues[];

    constructor(private issueService: IssuesService) { }

    public ngOnInit() { 
        // Load Issues
        this.issueService.getIssues().subscribe(x => {
            this.pendingIssues = x.filter(item => {
                return item.status === IssueStatus.Pending;
            });
            this.inProgressIssues = x.filter(item => {
                return item.status === IssueStatus.InProgress;
            });
            this.resolvedIssues = x.filter(item => {
                return item.status === IssueStatus.Resolved;
            });
        });
    }
}
