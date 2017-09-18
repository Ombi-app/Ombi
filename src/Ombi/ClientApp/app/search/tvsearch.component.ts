import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs/Subject';

import { SearchService } from '../services/search.service';
import { AuthService } from '../auth/auth.service';
import { RequestService } from '../services/request.service';
import { NotificationService } from '../services/notification.service';

import { ISearchTvResult } from '../interfaces/ISearchTvResult';
import { IRequestEngineResult } from '../interfaces/IRequestEngineResult';
import { TreeNode } from 'primeng/primeng';

@Component({
    selector: 'tv-search',
    templateUrl: './tvsearch.component.html',
    styleUrls: ['./../requests/tvrequests.component.scss'],
})
export class TvSearchComponent implements OnInit, OnDestroy {

    private subscriptions = new Subject<void>();
    searchText: string;
    searchChanged = new Subject<string>();
    tvResults: TreeNode[];
    result: IRequestEngineResult;
    searchApplied = false;

    constructor(private searchService: SearchService, private requestService: RequestService,
        private notificationService: NotificationService, private route: Router, private authService: AuthService) {

        this.searchChanged
            .debounceTime(600) // Wait Xms afterthe last event before emitting last event
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
                    });
            });
    }
    openClosestTab(el: any): void {
        let rowclass = "undefined";
        el = el.toElement;
        while (el.className != rowclass) {
            // Increment the loop to the parent node until we find the row we need
            el = el.parentNode;
            if (!el) {
            }
        }
        // At this point, the while loop has stopped and `el` represents the element that has
        // the class you specified

        // Then we loop through the children to find the caret which we want to click
        let caretright = "ui-treetable-toggler fa fa-fw ui-clickable fa-caret-right";
        let caretdown = "ui-treetable-toggler fa fa-fw ui-clickable fa-caret-down";
        for (let value of el.children) {
            // the caret from the ui has 2 class selectors depending on if expanded or not
            // we search for both since we want to still toggle the clicking
            if (value.className === caretright || value.className === caretdown) {
                // Then we tell JS to click the element even though we hid it from the UI
                value.click();
                //Break from loop since we no longer need to continue looking
                break;
            }
        };
    }
    
    ngOnInit(): void {
        this.searchText = "";
        this.tvResults = [];
        this.result = {
            message: "",
            requestAdded: false,
            errorMessage:""
        }
    }

    search(text: any) {
        this.searchChanged.next(text.target.value);
    }


    popularShows() {
        this.clearResults();
        this.searchService.popularTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    trendingShows() {
        this.clearResults();
        this.searchService.trendingTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    mostWatchedShows() {
        this.clearResults();
        this.searchService.mostWatchedTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    anticipatedShows() {
        this.clearResults();
        this.searchService.anticipatedTv()
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.tvResults = x;
                this.getExtraInfo();
            });
    }

    getExtraInfo() {
        this.tvResults.forEach((val, index) => {
            this.searchService.getShowInformationTreeNode(val.data.id)
                .takeUntil(this.subscriptions)
                .subscribe(x => {
                    this.updateItem(val.data, x);
                });
        });
    }

    request(searchResult: ISearchTvResult) {
        searchResult.requested = true;
        if (this.authService.hasRole("admin") || this.authService.hasRole("AutoApproveMovie")) {
            searchResult.approved = true;
        }
        this.requestService.requestTv(searchResult)
            .takeUntil(this.subscriptions)
            .subscribe(x => {
                this.result = x;
                if (this.result.requestAdded) {
                    this.notificationService.success("Request Added",
                        `Request for ${searchResult.title} has been added successfully`);
                } else {
                    this.notificationService.warning("Request Added", this.result.message);
                }
            });
    }

    allSeasons(searchResult: ISearchTvResult) {
        searchResult.requestAll = true;
        this.request(searchResult);
    }

    firstSeason(searchResult: ISearchTvResult) {
        searchResult.firstSeason = true;
        this.request(searchResult);
    }

    latestSeason(searchResult: ISearchTvResult) {
        searchResult.latestSeason = true;
        this.request(searchResult);
    }

    selectSeason(searchResult: ISearchTvResult) {
        this.route.navigate(['/search/show', searchResult.id]);
    }

    private updateItem(key: TreeNode, updated: TreeNode) {
        let item = this.tvResults.filter((val) => {
            return val.data == key;
        });
        item[0].data = updated.data;
    }

    private clearResults() {
        this.tvResults = [];
        this.searchApplied = false;
    }

    ngOnDestroy(): void {
        this.subscriptions.next();
        this.subscriptions.complete();
    }

}