import { Component, OnInit } from "@angular/core";

import { IIssueCategory } from "../interfaces";
import { IssuesService, SettingsService } from "../services";

@Component({
    templateUrl: "./search.component.html",
})
export class SearchComponent implements OnInit  {
    public showTv: boolean;
    public showMovie: boolean;
    public showMusic: boolean;
    public issueCategories: IIssueCategory[];
    public issuesEnabled = false;
    public musicEnabled: boolean;

    constructor(private issuesService: IssuesService,
                private settingsService: SettingsService) {

    }

    public ngOnInit() {
        this.settingsService.lidarrEnabled().subscribe(x => this.musicEnabled = x);
        this.showMovie = true;
        this.showTv = false;
        this.showMusic = false;
        this.issuesService.getCategories().subscribe(x => this.issueCategories = x);
        this.settingsService.getIssueSettings().subscribe(x => this.issuesEnabled = x.enabled);
    }

    public selectMovieTab() {
        this.showMovie = true;
        this.showTv = false;
        this.showMusic = false;
    }

    public selectTvTab() {
        this.showMovie = false;
        this.showTv = true;
        this.showMusic = false;
    }
    public selectMusicTab() {
        this.showMovie = false;
        this.showTv = false;
        this.showMusic = true;
    }
}
