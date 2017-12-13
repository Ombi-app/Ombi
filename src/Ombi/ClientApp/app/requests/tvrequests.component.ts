import { Component, Input, OnInit } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";
import { Subject } from "rxjs/Subject";
import { ImageService } from "./../services/image.service";

import "rxjs/add/operator/debounceTime";
import "rxjs/add/operator/distinctUntilChanged";
import "rxjs/add/operator/map";

import { AuthService } from "../auth/auth.service";
import { RequestService } from "../services";

import { TreeNode } from "primeng/primeng";
import { IIssueCategory, ITvRequests } from "../interfaces";

@Component({
    selector: "tv-requests",
    templateUrl: "./tvrequests.component.html",
    styleUrls: ["./tvrequests.component.scss"],
})
export class TvRequestsComponent implements OnInit {

    public tvRequests: TreeNode[];
    public searchChanged = new Subject<string>();
    public searchText: string;
    public isAdmin: boolean;
    public showChildDialogue = false; // This is for the child modal popup
    public selectedSeason: ITvRequests;

    @Input() public issueCategories: IIssueCategory[];
    @Input() public issuesEnabled: boolean;

    private currentlyLoaded: number;
    private amountToLoad: number;

    constructor(private requestService: RequestService,
                private auth: AuthService,
                private sanitizer: DomSanitizer,
                private imageService: ImageService) {
        this.searchChanged
            .debounceTime(600) // Wait Xms after the last event before emitting last event
            .distinctUntilChanged() // only emit if value is different from previous value
            .subscribe(x => {
                this.searchText = x as string;
                if (this.searchText === "") {
                    this.resetSearch();
                    return;
                }
                this.requestService.searchTvRequestsTree(this.searchText)
                    .subscribe(m => {
                        this.tvRequests = m;
                        this.tvRequests.forEach((val) => this.loadBackdrop(val));
                    });
            });
    }
    public openClosestTab(el: any) {
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
        this.amountToLoad = 1000;
        this.currentlyLoaded = 1000;
        this.tvRequests = [];
        this.loadInit();
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
    }

    public loadMore() {
        //TODO: I believe this +1 is causing off by one error skipping loading of tv shows
        //When removed and scrolling very slowly everything works as expected, however
        //if you scroll really quickly then you start getting duplicates of movies
        //since it's async and some subsequent results return first and then incrementer
        //is increased so you see movies which had already been gotten show up...
        this.requestService.getTvRequestsTree(this.amountToLoad, this.currentlyLoaded + 1)
            .subscribe(x => {
                this.tvRequests = x;
                this.currentlyLoaded = this.currentlyLoaded + this.amountToLoad;
            });
    }

    public search(text: any) {
        this.searchChanged.next(text.target.value);
    }

    public showChildren(request: ITvRequests) {
        this.selectedSeason = request;
        this.showChildDialogue = true;
    }

    public childRequestDeleted(childId: number): void {
        // Refresh the UI, hackly way around reloading the data
        this.ngOnInit();
    }

    private loadInit() {
        this.requestService.getTvRequestsTree(this.amountToLoad, 0)
            .subscribe(x => {
                this.tvRequests = x;
                this.tvRequests.forEach((val, index) => {
                    this.loadBackdrop(val);
            });     
        });
    }

    private resetSearch() {
        this.currentlyLoaded = 5;
        this.loadInit();
    }
    private loadBackdrop(val: TreeNode): void {
        this.imageService.getTvBanner(val.data.id).subscribe(x => {
            val.data.background = this.sanitizer.bypassSecurityTrustStyle
                ("linear-gradient(to bottom, rgba(0,0,0,0.6) 0%,rgba(0,0,0,0.6) 100%),url(" + x + ")");
            });
    }
}
