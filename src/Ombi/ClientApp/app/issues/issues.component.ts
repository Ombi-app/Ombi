import { Component, OnInit } from "@angular/core";

import { IssuesService } from "../services";

import { IIssues, IssueStatus } from "../interfaces";

@Component({
    templateUrl: "issues.component.html",
})
export class IssuesComponent implements OnInit {

    public issues: IIssues[];

    public IssueStatus = IssueStatus;

    public order: string = "movie.title";
    public reverse = false;

    constructor(private issueService: IssuesService) { }

    public ngOnInit() { 
        // Load Issues
        this.issueService.getIssues().subscribe(x => this.issues = x);
    }

    public setOrder(value: string) {
        if (this.order === value) {
          this.reverse = !this.reverse;
        }
    
        this.order = value;
      }
}
