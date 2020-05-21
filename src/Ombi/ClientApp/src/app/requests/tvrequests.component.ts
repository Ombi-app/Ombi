// import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
// import { Component, Input, OnInit, Inject } from "@angular/core";
// import { DomSanitizer } from "@angular/platform-browser";
// import { Subject } from "rxjs";
// import { debounceTime, distinctUntilChanged } from "rxjs/operators";

// import { AuthService } from "../auth/auth.service";
// import { FilterType, IIssueCategory, IPagenator, IRequestsViewModel, ISonarrProfile, ISonarrRootFolder, ITvRequests, OrderType } from "../interfaces";
// import { NotificationService, RequestService, SonarrService } from "../services";
// import { ImageService } from "../services/image.service";

// @Component({
//     selector: "tv-requests",
//     templateUrl: "./tvrequests.component.html",
//     styleUrls: ["./tvrequests.component.scss"],
// })
// export class TvRequestsComponent implements OnInit {

//     public tvRequests: IRequestsViewModel<ITvRequests>;
//     public searchChanged = new Subject<string>();
//     public searchText: string;
//     public isAdmin: boolean;
//     public currentUser: string;
//     public showChildDialogue = false; // This is for the child modal popup
//     public selectedSeason: ITvRequests;
//     public defaultPoster: string;

//     @Input() public issueCategories: IIssueCategory[];
//     @Input() public issuesEnabled: boolean;
//     public issueProviderId: string;
//     public issuesBarVisible = false;
//     public issueRequest: ITvRequests;
//     public issueCategorySelected: IIssueCategory;

//     public sonarrProfiles: ISonarrProfile[] = [];
//     public sonarrRootFolders: ISonarrRootFolder[] = [];

//     public totalTv: number = 100;
//     private currentlyLoaded: number;
//     private amountToLoad: number;
//     private href: string;

//     constructor(
//         private requestService: RequestService,
//         private auth: AuthService,
//         private sanitizer: DomSanitizer,
//         private imageService: ImageService,
//         private sonarrService: SonarrService,
//         private notificationService: NotificationService,
//          @Inject(APP_BASE_HREF) href:string) {
//             this.href= href;
//             this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
//             this.currentUser = this.auth.claims().name;
//             if (this.isAdmin) {
//                 this.sonarrService.getQualityProfilesWithoutSettings()
//                     .subscribe(x => this.sonarrProfiles = x);
        
//                 this.sonarrService.getRootFoldersWithoutSettings()
//                     .subscribe(x => this.sonarrRootFolders = x);
//             }
//     }

//     public openClosestTab(node: ITvRequests,el: any) {
//         el.preventDefault();
//         node.open = !node.open;
//     }

//     public ngOnInit() {
//         this.amountToLoad = 10;
//         this.currentlyLoaded = 10;
//         this.tvRequests = {collection:[], total:0};

//         this.searchChanged.pipe(
//             debounceTime(600), // Wait Xms after the last event before emitting last event
//             distinctUntilChanged(), // only emit if value is different from previous value
//         ).subscribe(x => {
//             this.searchText = x as string;
//             if (this.searchText === "") {
//                 this.resetSearch();
//                 return;
//             }
//             this.requestService.searchTvRequests(this.searchText)
//                 .subscribe(m => {
//                     this.tvRequests.collection = m;
//                     this.tvRequests.collection.forEach((val) => this.loadBackdrop(val));
//                     this.tvRequests.collection.forEach((val) => this.setOverride(val));
//                 });
//         });
//         this.defaultPoster = "../../../images/default_tv_poster.png";
//         const base = this.href;
//         if (base) {
//             this.defaultPoster = "../../.." + base + "/images/default_tv_poster.png";
//         }

//         this.loadInit();
//     }

//     public paginate(event: IPagenator) {
//         const skipAmount = event.first;

//         this.requestService.getTvRequests(this.amountToLoad, skipAmount, OrderType.RequestedDateDesc, FilterType.None, FilterType.None)
//             .subscribe(x => {
//                 this.tvRequests = x;
//                 this.currentlyLoaded = this.currentlyLoaded + this.amountToLoad;
//             });
//     }

//     public search(text: any) {
//         this.searchChanged.next(text.target.value);
//     }

//     public showChildren(request: ITvRequests) {
//         this.selectedSeason = request;
//         this.showChildDialogue = true;
//     }

//     public childRequestDeleted(childId: number): void {
//         // Refresh the UI, hackly way around reloading the data
//         this.ngOnInit();
//     }

//     public selectRootFolder(searchResult: ITvRequests, rootFolderSelected: ISonarrRootFolder, event: any) {
//         event.preventDefault();
//         searchResult.rootFolder = rootFolderSelected.id;
//         this.setOverride(searchResult);
//         this.setRootFolder(searchResult);
//     }

//     public selectQualityProfile(searchResult: ITvRequests, profileSelected: ISonarrProfile, event: any) {
//         event.preventDefault();
//         searchResult.qualityOverride = profileSelected.id;
//         this.setOverride(searchResult);
//         this.setQualityProfile(searchResult);
//     }

//     public reportIssue(catId: IIssueCategory, req: ITvRequests) {
//         this.issueRequest = req;
//         this.issueCategorySelected = catId;
//         this.issuesBarVisible = true;
//         this.issueProviderId = req.id.toString();
//     }

//     private setOverride(req: ITvRequests): void {
//         this.setQualityOverrides(req);
//         this.setRootFolderOverrides(req);
//     }

//     private setQualityProfile(req: ITvRequests) {
//         this.requestService.setQualityProfile(req.id, req.qualityOverride).subscribe(x => {
//             if(x) {
//                 this.notificationService.success("Quality profile updated");
//             } else {
//                 this.notificationService.error("Could not update the quality profile");
//             }
//         });
//     }

//     private setRootFolder(req: ITvRequests) {
//         this.requestService.setRootFolder(req.id, req.rootFolder).subscribe(x => {
//             if(x) {
//                 this.notificationService.success("Quality profile updated");
//             } else {
//                 this.notificationService.error("Could not update the quality profile");
//             }
//         });
//     }

//     private setQualityOverrides(req: ITvRequests): void {
//         if (this.sonarrProfiles) {
//             const profile = this.sonarrProfiles.filter((p) => {
//                 return p.id === req.qualityOverride;
//             });
//             if (profile.length > 0) {
//                 req.qualityOverrideTitle = profile[0].name;
//             }
//         }
//     }
//     private setRootFolderOverrides(req: ITvRequests): void {
//         if (this.sonarrRootFolders) {
//             const path = this.sonarrRootFolders.filter((folder) => {
//                 return folder.id === req.rootFolder;
//             });
//             if (path.length > 0) {
//                 req.rootPathOverrideTitle = path[0].path;
//             }
//         }
//     }

//     private loadInit() {
//         this.requestService.getTotalTv().subscribe(x => this.totalTv = x);
//         this.requestService.getTvRequests(this.amountToLoad, 0, OrderType.RequestedDateDesc, FilterType.None, FilterType.None)
//             .subscribe(x => {
//                 this.tvRequests = x;
//                 this.tvRequests.collection.forEach((val, index) => {
//                     this.setDefaults(val);
//                     this.loadBackdrop(val);
//                     this.setOverride(val);
//                 });
//             });
//     }

//     private resetSearch() {
//         this.currentlyLoaded = 5;
//         this.loadInit();
//     }

//     private setDefaults(val: ITvRequests) {
//         if (val.posterPath === null) {
//             val.posterPath = this.defaultPoster;
//         }
//     }

//     private loadBackdrop(val: ITvRequests): void {
//         if (val.background != null) {
//             val.background = this.sanitizer.bypassSecurityTrustStyle
//                 ("url(https://image.tmdb.org/t/p/w1280" + val.background + ")");
//         } else {
//             this.imageService.getTvBanner(val.tvDbId).subscribe(x => {
//                 if (x) {
//                     val.background = this.sanitizer.bypassSecurityTrustStyle
//                         ("url(" + x + ")");
//                 }
//             });
//         }
//     }
// }
