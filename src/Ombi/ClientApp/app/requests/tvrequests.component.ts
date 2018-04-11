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
import { NotificationService, RequestService, SonarrService } from "../services";

import { TreeNode } from "primeng/primeng";
import { IIssueCategory, ISonarrProfile,  ISonarrRootFolder, ITvRequests } from "../interfaces";

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
    public issueProviderId: string;
    public issuesBarVisible = false;
    public issueRequest: ITvRequests;
    public issueCategorySelected: IIssueCategory;

    public sonarrProfiles: ISonarrProfile[] = [];
    public sonarrRootFolders: ISonarrRootFolder[] = [];

    private currentlyLoaded: number;
    private amountToLoad: number;

    constructor(private requestService: RequestService,
                private auth: AuthService,
                private sanitizer: DomSanitizer,
                private imageService: ImageService,
                private sonarrService: SonarrService,
                private notificationService: NotificationService) {
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
                        this.tvRequests.forEach((val) => this.setOverride(val.data));
                    });
            });
    }

    public openClosestTab(el: any) {
        const rowclass = "undefined ng-star-inserted";
        el = el.toElement || el.relatedTarget || el.target || el.srcElement;

        if (el.nodeName === "BUTTON") {

            const isButtonAlreadyActive = el.parentElement.querySelector(".active");
            // if a Button already has Class: .active
            if (isButtonAlreadyActive) {
                isButtonAlreadyActive.classList.remove("active");
            } else {
                el.className += " active";
            }
        }

        while (el.className !== rowclass) {
            // Increment the loop to the parent node until we find the row we need
            el = el.parentNode;
        }
        // At this point, the while loop has stopped and `el` represents the element that has
        // the class you specified

        // Then we loop through the children to find the caret which we want to click
        const caretright = "fa-caret-right";
        const caretdown = "fa-caret-down";
        for (const value of el.children) {
            // the caret from the ui has 2 class selectors depending on if expanded or not
            // we search for both since we want to still toggle the clicking
            if (value.className.includes(caretright) || value.className.includes(caretdown)) {
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
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        
        this.loadInit();
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

    public selectRootFolder(searchResult: ITvRequests, rootFolderSelected: ISonarrRootFolder, event: any) {
        event.preventDefault();
        searchResult.rootFolder = rootFolderSelected.id;
        this.setOverride(searchResult);
        this.updateRequest(searchResult);
    }

    public selectQualityProfile(searchResult: ITvRequests, profileSelected: ISonarrProfile, event: any) {
        event.preventDefault();
        searchResult.qualityOverride = profileSelected.id;
        this.setOverride(searchResult);
        this.updateRequest(searchResult);
    }

    public reportIssue(catId: IIssueCategory, req: ITvRequests) {
        this.issueRequest = req;
        this.issueCategorySelected = catId;
        this.issuesBarVisible = true;
        this.issueProviderId = req.id.toString();
    }

    private setOverride(req: ITvRequests): void {
        this.setQualityOverrides(req);
        this.setRootFolderOverrides(req);
    }

    private updateRequest(request: ITvRequests) {
        this.requestService.updateTvRequest(request)
            .subscribe(x => {
                this.notificationService.success("Request Updated");
                this.setOverride(x);
                request = x;
            });
    }

    private setQualityOverrides(req: ITvRequests): void {
        if (this.sonarrProfiles) {
            const profile = this.sonarrProfiles.filter((p) => {
                return p.id === req.qualityOverride;
            });
            if (profile.length > 0) {
                req.qualityOverrideTitle = profile[0].name;
            }
        }
    }
    private setRootFolderOverrides(req: ITvRequests): void {
        if (this.sonarrRootFolders) {
            const path = this.sonarrRootFolders.filter((folder) => {
                return folder.id === req.rootFolder;
            });
            if (path.length > 0) {
                req.rootPathOverrideTitle = path[0].path;
            }
        }
    }

    private loadInit() {
        this.requestService.getTvRequestsTree(this.amountToLoad, 0)
            .subscribe(x => {
                this.tvRequests = x;
                this.tvRequests.forEach((val, index) => {
                    this.setDefaults(val);
                    this.loadBackdrop(val);
                    this.setOverride(val.data);
            });     
        });

        if(this.isAdmin) {
            this.sonarrService.getQualityProfilesWithoutSettings()
                .subscribe(x => this.sonarrProfiles = x);
        
            this.sonarrService.getRootFoldersWithoutSettings()
                .subscribe(x => this.sonarrRootFolders = x);
        }
    }

    private resetSearch() {
        this.currentlyLoaded = 5;
        this.loadInit();
    }

    private setDefaults(val: any) {
        if (val.data.posterPath === null) {
            val.data.posterPath = "../../../images/default_tv_poster.png";
        }

        val.data.imdbId = "http://www.imdb.com/title/" + val.data.imdbId + "/";
    }

    private loadBackdrop(val: TreeNode): void {
        this.imageService.getTvBanner(val.data.tvDbId).subscribe(x => {
            val.data.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + x + ")");
            });
    }
}
