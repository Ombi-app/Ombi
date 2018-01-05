import { Component, OnInit } from "@angular/core";

import { IIssueCategory } from "./../interfaces";
import { IssuesService, SettingsService } from "./../services";

@Component({
    templateUrl: "./search.component.html",
})
export class SearchComponent implements OnInit  {
    public showTv: boolean;
    public showMovie: boolean;
    public issueCategories: IIssueCategory[];
    public issuesEnabled = false;

    constructor(private issuesService: IssuesService,
                private settingsService: SettingsService) {

    }

    public ngOnInit() {
        this.showMovie = true;
        this.showTv = false;
        this.issuesService.getCategories().subscribe(x => this.issueCategories = x);
        this.settingsService.getIssueSettings().subscribe(x => this.issuesEnabled = x.enabled);
    }

    public selectMovieTab() {
        this.showMovie = true;
        this.showTv = false;
    }

    public selectTvTab() {
        this.showMovie = false;
        this.showTv = true;
    }
}
