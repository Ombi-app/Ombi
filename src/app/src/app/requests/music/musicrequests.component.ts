// import { PlatformLocation, APP_BASE_HREF } from "@angular/common";
// import { Component, Input, OnInit, Inject } from "@angular/core";
// import { DomSanitizer } from "@angular/platform-browser";
// import { Subject } from "rxjs";
// import { debounceTime, distinctUntilChanged } from "rxjs/operators";

// import { AuthService } from "../../auth/auth.service";
// import { FilterType, IAlbumRequest, IFilter, IIssueCategory, IPagenator, OrderType } from "../../interfaces";
// import { NotificationService, RequestService } from "../../services";

// @Component({
//     selector: "music-requests",
//     templateUrl: "./musicrequests.component.html",
// })
// export class MusicRequestsComponent implements OnInit {
//     public albumRequests: IAlbumRequest[];
//     public defaultPoster: string;

//     public searchChanged: Subject<string> = new Subject<string>();
//     public searchText: string;

//     public isAdmin: boolean; // Also PowerUser

//     @Input() public issueCategories: IIssueCategory[];
//     @Input() public issuesEnabled: boolean;
//     public issuesBarVisible = false;
//     public issueRequest: IAlbumRequest;
//     public issueProviderId: string;
//     public issueCategorySelected: IIssueCategory;

//     public filterDisplay: boolean;
//     public filter: IFilter;
//     public filterType = FilterType;

//     public orderType: OrderType = OrderType.RequestedDateDesc;
//     public OrderType = OrderType;
//     public denyDisplay: boolean;
//     public requestToDeny: IAlbumRequest;
//     public rejectionReason: string;

//     public totalAlbums: number = 100;
//     public currentlyLoaded: number;
//     private amountToLoad: number;
//     private href: string;

//     constructor(
//         private requestService: RequestService,
//         private auth: AuthService,
//         private notificationService: NotificationService,
//         private sanitizer: DomSanitizer,
//          @Inject(APP_BASE_HREF) href:string) {
//             this.href = href;
//         this.searchChanged.pipe(
//             debounceTime(600), // Wait Xms after the last event before emitting last event
//             distinctUntilChanged(), // only emit if value is different from previous value
//         ).subscribe(x => {
//             this.searchText = x as string;
//             if (this.searchText === "") {
//                 this.resetSearch();
//                 return;
//             }
//             this.requestService.searchAlbumRequests(this.searchText)
//                 .subscribe(m => {
//                     this.setOverrides(m);
//                     this.albumRequests = m;
//                 });
//         });
//         this.defaultPoster = "../../../images/default-music-placeholder.png";
//         const base = this.href;
//         if (base) {
//             this.defaultPoster = "../../.." + base + "/images/default-music-placeholder.png";
//         }
//     }

//     public ngOnInit() {
//         this.amountToLoad = 10;
//         this.currentlyLoaded = 10;
//         this.filter = {
//             availabilityFilter: FilterType.None,
//             statusFilter: FilterType.None,
//         };
//         this.loadInit();
//         this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
//     }

//     public paginate(event: IPagenator) {
//         const skipAmount = event.first;
//         this.loadRequests(this.amountToLoad, skipAmount);
//     }

//     public search(text: any) {
//         this.searchChanged.next(text.target.value);
//     }

//     public async removeRequest(request: IAlbumRequest) {
//         await this.requestService.removeAlbumRequest(request).toPromise();
//         this.removeRequestFromUi(request);
//         this.loadRequests(this.amountToLoad, this.currentlyLoaded = 0);
//     }

//     public changeAvailability(request: IAlbumRequest, available: boolean) {
//         request.available = available;

//         if (available) {
//             this.requestService.markAlbumAvailable({ id: request.id }).subscribe(x => {
//                 if (x.result) {
//                     this.notificationService.success(
//                         `${request.title} Is now available`);
//                 } else {
//                     this.notificationService.warning("Request Available", x.message ? x.message : x.errorMessage);
//                     request.approved = false;
//                 }
//             });
//         } else {
//             this.requestService.markAlbumUnavailable({ id: request.id }).subscribe(x => {
//                 if (x.result) {
//                     this.notificationService.success(
//                         `${request.title} Is now unavailable`);
//                 } else {
//                     this.notificationService.warning("Request Available", x.message ? x.message : x.errorMessage);
//                     request.approved = false;
//                 }
//             });
//         }
//     }

//     public approve(request: IAlbumRequest) {
//         request.approved = true;
//         this.approveRequest(request);
//     }

//     public deny(request: IAlbumRequest) {
//         this.requestToDeny = request;
//         this.denyDisplay = true;
//     }

//     public denyRequest() {
//         this.requestService.denyAlbum({ id: this.requestToDeny.id, reason: this.rejectionReason })
//             .subscribe(x => {
//                 if (x.result) {
//                     this.notificationService.success(
//                         `Request for ${this.requestToDeny.title} has been denied successfully`);
//                 } else {
//                     this.notificationService.warning("Request Denied", x.message ? x.message : x.errorMessage);
//                     this.requestToDeny.denied = false;
//                 }
//             });
//     }

//     public reportIssue(catId: IIssueCategory, req: IAlbumRequest) {
//         this.issueRequest = req;
//         this.issueCategorySelected = catId;
//         this.issuesBarVisible = true;
//         this.issueProviderId = req.foreignAlbumId;
//     }

//     public ignore(event: any): void {
//         event.preventDefault();
//     }

//     public clearFilter(el: any) {
//         el = el.toElement || el.relatedTarget || el.target || el.srcElement;

//         el = el.parentElement;
//         el = el.querySelectorAll("INPUT");
//         for (el of el) {
//             el.checked = false;
//             el.parentElement.classList.remove("active");
//         }

//         this.filterDisplay = false;
//         this.filter.availabilityFilter = FilterType.None;
//         this.filter.statusFilter = FilterType.None;

//         this.resetSearch();
//     }

//     public filterAvailability(filter: FilterType, el: any) {
//         this.filterActiveStyle(el);
//         this.filter.availabilityFilter = filter;
//         this.loadInit();
//     }

//     public filterStatus(filter: FilterType, el: any) {
//         this.filterActiveStyle(el);
//         this.filter.statusFilter = filter;
//         this.loadInit();
//     }

//     public setOrder(value: OrderType, el: any) {
//         el = el.toElement || el.relatedTarget || el.target || el.srcElement;

//         const parent = el.parentElement;
//         const previousFilter = parent.querySelector(".active");

//         previousFilter.className = "";
//         el.className = "active";

//         this.orderType = value;

//         this.loadInit();
//     }

//     public isRequestUser(request: IAlbumRequest) {
//         if (request.requestedUser.userName === this.auth.claims().name) {
//             return true;
//         }
//         return false;
//     }

//     // public subscribe(request: IAlbumRequest) {
//     //     request.subscribed = true;
//     //     this.requestService.subscribeToMovie(request.id)
//     //         .subscribe(x => {
//     //             this.notificationService.success("Subscribed To Movie!");
//     //         });
//     // }

//     // public unSubscribe(request: IMovieRequests) {
//     //     request.subscribed = false;
//     //     this.requestService.unSubscribeToMovie(request.id)
//     //         .subscribe(x => {
//     //             this.notificationService.success("Unsubscribed Movie!");
//     //         });
//     // }

//     private filterActiveStyle(el: any) {
//         el = el.toElement || el.relatedTarget || el.target || el.srcElement;

//         el = el.parentElement; //gets radio div
//         el = el.parentElement; //gets form group div
//         el = el.parentElement; //gets status filter div
//         el = el.querySelectorAll("INPUT");
//         for (el of el) {
//             if (el.checked) {
//                 if (!el.parentElement.classList.contains("active")) {
//                     el.parentElement.className += " active";
//                 }
//             } else {
//                 el.parentElement.classList.remove("active");
//             }
//         }
//     }

//     private loadRequests(amountToLoad: number, currentlyLoaded: number) {
//         this.requestService.getAlbumRequests(amountToLoad, currentlyLoaded, this.orderType, this.filter)
//             .subscribe(x => {
//                 this.setOverrides(x.collection);
//                 if (!this.albumRequests) {
//                     this.albumRequests = [];
//                 }
//                 this.albumRequests = x.collection;
//                 this.totalAlbums = x.total;
//                 this.currentlyLoaded = currentlyLoaded + amountToLoad;
//             });
//     }

//     private approveRequest(request: IAlbumRequest) {
//         this.requestService.approveAlbum({ id: request.id })
//             .subscribe(x => {
//                 request.approved = true;
//                 if (x.result) {
//                     this.notificationService.success(
//                         `Request for ${request.title} has been approved successfully`);
//                 } else {
//                     this.notificationService.warning("Request Approved", x.message ? x.message : x.errorMessage);
//                     request.approved = false;
//                 }
//             });
//     }

//     private loadInit() {
//         this.requestService.getAlbumRequests(this.amountToLoad, 0, this.orderType, this.filter)
//             .subscribe(x => {
//                 this.albumRequests = x.collection;
//                 this.totalAlbums = x.total;

//                 this.setOverrides(this.albumRequests);

//                 if (this.isAdmin) {
//                     // this.radarrService.getQualityProfilesFromSettings().subscribe(c => {
//                     //     this.radarrProfiles = c;
//                     //     this.albumRequests.forEach((req) => this.setQualityOverrides(req));
//                     // });
//                     // this.radarrService.getRootFoldersFromSettings().subscribe(c => {
//                     //     this.radarrRootFolders = c;
//                     //     this.albumRequests.forEach((req) => this.setRootFolderOverrides(req));
//                     // });
//                 }
//             });
//     }

//     private resetSearch() {
//         this.currentlyLoaded = 5;
//         this.loadInit();
//     }

//     private removeRequestFromUi(key: IAlbumRequest) {
//         const index = this.albumRequests.indexOf(key, 0);
//         if (index > -1) {
//             this.albumRequests.splice(index, 1);
//         }
//     }

//     private setOverrides(requests: IAlbumRequest[]): void {
//         requests.forEach((req) => {
//             this.setOverride(req);
//         });
//     }

//     // private setQualityOverrides(req: IMovieRequests): void {
//     //     if (this.radarrProfiles) {
//     //         const profile = this.radarrProfiles.filter((p) => {
//     //             return p.id === req.qualityOverride;
//     //         });
//     //         if (profile.length > 0) {
//     //             req.qualityOverrideTitle = profile[0].name;
//     //         }
//     //     }
//     // }
//     // private setRootFolderOverrides(req: IMovieRequests): void {
//     //     if (this.radarrRootFolders) {
//     //         const path = this.radarrRootFolders.filter((folder) => {
//     //             return folder.id === req.rootPathOverride;
//     //         });
//     //         if (path.length > 0) {
//     //             req.rootPathOverrideTitle = path[0].path;
//     //         }
//     //     }
//     // }

//     private setOverride(req: IAlbumRequest): void {
//         this.setAlbumBackground(req);
//         // this.setQualityOverrides(req);
//         // this.setRootFolderOverrides(req);
//     }
//     private setAlbumBackground(req: IAlbumRequest) {
//         if (req.disk === null) {
//             if (req.cover === null) {
//                 req.disk = this.defaultPoster;
//             } else {
//                 req.disk = req.cover;
//             }
//         }
//         req.background = this.sanitizer.bypassSecurityTrustStyle
//             ("url(" + req.cover + ")");
//     }

// }
