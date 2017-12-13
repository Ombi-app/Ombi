import { Component, OnInit } from "@angular/core";

import { IssuesService } from "../services";

import { IMovieIssues, IssueStatus, ITvIssues } from "../interfaces";

@Component({
    templateUrl: "issues.component.html",
})
export class IssuesComponent implements OnInit {

    public movieIssues: IMovieIssues[];
    public tvIssues: ITvIssues[];

    public IssueStatus = IssueStatus;

    public order: string = "movie.title";
    public reverse = false;

    constructor(private issueService: IssuesService) { }

    public ngOnInit() { 
        // Load Issues
        this.issueService.getMovieIssues().subscribe(x => this.movieIssues = x);
        this.issueService.getTvIssues().subscribe(x => this.tvIssues = x);
    }

    public setOrder(value: string) {
        if (this.order === value) {
          this.reverse = !this.reverse;
        }
    
        this.order = value;
      }
}
