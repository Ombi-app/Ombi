
// import { Component, OnInit } from "@angular/core";

// import { IIssueCategory } from "../interfaces";
// import { IssuesService, SettingsService } from "../services";

// @Component({
//     templateUrl: "./request.component.html",
// })
// export class RequestComponent implements OnInit  {

//     public showMovie = true;
//     public showTv = false;
//     public showAlbums = false;

//     public issueCategories: IIssueCategory[];
//     public issuesEnabled = false;
//     public musicEnabled: boolean;

//     constructor(private issuesService: IssuesService,
//                 private settingsService: SettingsService) {

//     }

//     public ngOnInit(): void {
//         this.issuesService.getCategories().subscribe(x => this.issueCategories = x);
//         this.settingsService.lidarrEnabled().subscribe(x => this.musicEnabled = x);
//         this.settingsService.getIssueSettings().subscribe(x => this.issuesEnabled = x.enabled);
//     }

//     public selectMovieTab() {
//         this.showMovie = true;
//         this.showTv = false;
//         this.showAlbums = false;
//     }

//     public selectTvTab() {
//         this.showMovie = false;
//         this.showTv = true;
//         this.showAlbums = false;
//     }

//     public selectMusicTab() {
//         this.showMovie = false;
//         this.showTv = false;
//         this.showAlbums = true;
//     }
// }
