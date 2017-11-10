﻿import { Component, OnDestroy, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { Router } from "@angular/router";
import { Subject } from "rxjs/Subject";

import { AuthService } from "../auth/auth.service";
import { ImageService, NotificationService, RequestService, SearchService} from "../services";

import { TreeNode } from "primeng/primeng";
import { IRequestEngineResult } from "../interfaces";
import { ISearchTvResult } from "../interfaces";

@Component({
    selector: "tv-search",
    templateUrl: "./tvsearch.component.html",
    styleUrls: ["./../requests/tvrequests.component.scss"],
})
export class TvSearchComponent implements OnInit, OnDestroy {

    public searchText: string;
    public searchChanged = new Subject<string>();
    public tvResults: TreeNode[];
    public result: IRequestEngineResult;
    public searchApplied = false;

    private subscriptions = new Subject<void>();

    constructor(private searchService: SearchService, private requestService: RequestService,
                private notificationService: NotificationService, private route: Router, private authService: AuthService,
                private imageService: ImageService, private sanitizer: DomSanitizer) {

        this.searchChanged
            .debounceTime(600) // Wait Xms after the last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.clearResults();
                    return;
                }
                this.searchService.searchTvTreeNode(this.searchText)
                    .takeUntil(this.subscriptions)
                    .subscribe(x => {
                        this.tvResults = x;
                        this.searchApplied = true;
                        this.getExtraInfo();
                    });
            });
    }
    public openClosestTab(el: any) {
        el.preventDefault();
        const rowclass = "undefined";
        el = el.toElement || el.relatedTarget || el.target;
        while (el.className !== rowclass) {
            // Increment the loop to the parent node until we find the row we need
            el = el.parentNode;
        }
        // At this point, the while loop has stopped and `el` represents the element that has
        // the class you specified

        // Then we loop through the children to find the caret which we want to click
        const caretright = "ui-treetable-toggler fa fa-fw ui-clickable fa-caret-right";
        const caretdown = "ui-treetable-toggler fa fa-fw ui-clickable fa-caret-down";
        for (const value of el.children) {
            // the caret from the ui has 2 class selectors depending on if expanded or not
            // we search for both since we want to still toggle the clicking
            if (value.className === caretright || value.className === caretdown) {
                // Then we tell JS to click the element even though we hid it from the UI
                value.click();
                //Break from loop since we no longer need to continue looking
                break;
            }
        }
    }

    public ngOnInit() {
        this.searchText = "";
        this.tvResults = [];
        this.result = {
            message: "",
            result: false,
            errorMessage:"",
        };
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public popularShows() {
        this.clearResults();
        this.searchService.popularTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public trendingShows() {
        this.clearResults();
        this.searchService.trendingTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public mostWatchedShows() {
        this.clearResults();
        this.searchService.mostWatchedTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public anticipatedShows() {
        this.clearResults();
        this.searchService.anticipatedTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    public getExtraInfo() {
        this.tvResults.forEach((val, index) => {

            this.imageService.getTvBanner(val.data.id).subscribe(x => {
                
                val.data.background = this.sanitizer.
                bypassSecurityTrustStyle
                ("linear-gradient(to bottom, rgba(0,0,0,0.6) 0%,rgba(0,0,0,0.6) 100%),url(" + x + ")");
            });
            this.searchService.getShowInformationTreeNode(val.data.id)
                .takeUntil(this.subscriptions)
                .subscribe(x => {
                    if (x.data) {
                        this.updateItem(val, x);
                    } else {
                        const index = this.tvResults.indexOf(val, 0);
                        if (index > -1) {
                            this.tvResults.splice(index, 1);
                        }
                    }
                });
        });
    }

    public request(searchResult: ISearchTvResult) {
        searchResult.requested = true;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMovie")) {
            searchResult.approved = true;
        }
        this.requestService.requestTv(searchResult)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.result = x;
                if (this.result.result) {
                    this.notificationService.success(
                        `Request for ${searchResult.title} has been added successfully`);
                } else {
                    if (this.result.errorMessage && this.result.message) {
                        this.notificationService.warning("Request Added", `${this.result.message} - ${this.result.errorMessage}`);
                    } else {
                        this.notificationService.warning("Request Added", this.result.message ? this.result.message : this.result.errorMessage);
                    }
                }
            });
    }

    public allSeasons(searchResult: ISearchTvResult, event: any) {
        event.preventDefault();
        searchResult.requestAll = true;
        this.request(searchResult);
    }

    public firstSeason(searchResult: ISearchTvResult, event: any) {
        event.preventDefault();
        searchResult.firstSeason = true;
        this.request(searchResult);
    }

    public latestSeason(searchResult: ISearchTvResult, event: any) {
        event.preventDefault();
        searchResult.latestSeason = true;
        this.request(searchResult);
    }

    public selectSeason(searchResult: ISearchTvResult) {
        this.route.navigate(["/search/show", searchResult.id]);
    }

    public ngOnDestroy() {
        this.subscriptions.next();
        this.subscriptions.complete();
    }

    private updateItem(key: TreeNode, updated: TreeNode) {
        const index = this.tvResults.indexOf(key, 0);
        if (index > -1) {
            // Update certain properties, otherwise we will loose some data
            this.tvResults[index].data.banner = updated.data.banner;
            this.tvResults[index].data.imdbId = updated.data.imdbId;
            this.tvResults[index].data.seasonRequests = updated.data.seasonRequests;
            this.tvResults[index].data.seriesId = updated.data.seriesId;
            this.tvResults[index].data.fullyAvailable = updated.data.fullyAvailable;
            this.tvResults[index].data.backdrop = updated.data.backdrop;
        }
    }

    private clearResults() {
        this.tvResults = [];
        this.searchApplied = false;
    }
}
