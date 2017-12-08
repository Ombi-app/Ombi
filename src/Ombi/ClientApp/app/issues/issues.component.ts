import { Component, OnInit } from "@angular/core";

import { IssuesService } from "../services";

import { IMovieIssues, ITvIssues } from "../interfaces";

@Component({
    templateUrl: "issues.component.html",
})
export class IssuesComponent implements OnInit {

    public movieIssues: IMovieIssues[];
    public tvIssues: ITvIssues[];
    public orderValue: string;

    constructor(private issueService: IssuesService) { }

    public ngOnInit() { 
        // Load Issues
        this.issueService.getMovieIssues().subscribe(x => this.movieIssues = x);
        this.issueService.getTvIssues().subscribe(x => this.tvIssues = x);
    }

    public order(title: string) {
        this.orderValue = title;
    }
}
