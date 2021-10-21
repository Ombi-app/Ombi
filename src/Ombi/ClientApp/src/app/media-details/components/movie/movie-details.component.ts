import { AfterViewInit, Component, ViewChild, ViewEncapsulation } from "@angular/core";
import { ImageService, SearchV2Service, RequestService, MessageService, RadarrService, SettingsStateService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchMovieResultV2 } from "../../../interfaces/ISearchMovieResultV2";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { AuthService } from "../../../auth/auth.service";
import { IMovieRequests, RequestType, IAdvancedData } from "../../../interfaces";
import { DenyDialogComponent } from "../shared/deny-dialog/deny-dialog.component";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { TranslateService } from "@ngx-translate/core";
import { MovieAdvancedOptionsComponent } from "./panels/movie-advanced-options/movie-advanced-options.component";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { RequestBehalfComponent } from "../shared/request-behalf/request-behalf.component";
import { forkJoin } from "rxjs";
import { AdminRequestDialogComponent } from "../../../shared/admin-request-dialog/admin-request-dialog.component";

@Component({
    templateUrl: "./movie-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class MovieDetailsComponent {
    public movie: ISearchMovieResultV2;
    public hasRequest: boolean;
    public movieRequest: IMovieRequests;
    public isAdmin: boolean;
    public advancedOptions: IAdvancedData;
    public showAdvanced: boolean; // Set on the UI
    public issuesEnabled: boolean;

    public requestType = RequestType.movie;


    private theMovidDbId: number;
    private imdbId: string;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer, private imageService: ImageService,
        public dialog: MatDialog, private requestService: RequestService,
        private requestService2: RequestServiceV2, private radarrService: RadarrService,
        public messageService: MessageService, private auth: AuthService, private settingsState: SettingsStateService,
        private translate: TranslateService) {
        this.route.params.subscribe(async (params: any) => {
            if (typeof params.movieDbId === 'string' || params.movieDbId instanceof String) {
                if (params.movieDbId.startsWith("tt")) {
                    this.imdbId = params.movieDbId;
                }
            }
            this.theMovidDbId = params.movieDbId;
            await this.load();
        });
    }

    public async load() {

        this.issuesEnabled = this.settingsState.getIssue();
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");

        if (this.isAdmin) {
            this.showAdvanced = await this.radarrService.isRadarrEnabled();
        }

        if (this.imdbId) {
            this.searchService.getMovieByImdbId(this.imdbId).subscribe(async x => {
                this.movie = x;
                if (this.movie.requestId > 0) {
                    // Load up this request
                    this.hasRequest = true;
                    this.movieRequest = await this.requestService.getMovieRequest(this.movie.requestId);
                }
                this.loadBanner();
            });
        } else {
            this.searchService.getFullMovieDetails(this.theMovidDbId).subscribe(async x => {
                this.movie = x;
                if (this.movie.requestId > 0) {
                    // Load up this request
                    this.hasRequest = true;
                    this.movieRequest = await this.requestService.getMovieRequest(this.movie.requestId);
                    this.loadAdvancedInfo();
                }
                this.loadBanner();
            });
        }
    }

    public async request(userId?: string) {
        if (this.isAdmin) {
            const dialog = this.dialog.open(AdminRequestDialogComponent, { width: "700px", data: { type: RequestType.movie, id: this.movie.id }, panelClass: 'modal-panel' });
            dialog.afterClosed().subscribe(async (result) => {
                if (result) {
                    const requestResult = await this.requestService.requestMovie({ theMovieDbId: this.theMovidDbId,
                        languageCode: navigator.language,
                        qualityPathOverride: result.radarrPathId,
                        requestOnBehalf: result.username?.id,
                        rootFolderOverride: result.radarrFolderId, }).toPromise();
                    if (requestResult.result) {
                        this.movie.requested = true;
                        this.movie.requestId = requestResult.requestId;
                        this.messageService.send(this.translate.instant("Requests.RequestAddedSuccessfully", { title: this.movie.title }), "Ok");
                        this.movieRequest = await this.requestService.getMovieRequest(this.movie.requestId);
                    } else {
                        this.messageService.send(requestResult.errorMessage, "Ok");
                    }
                }
            });
        } else {
        const result = await this.requestService.requestMovie({ theMovieDbId: this.theMovidDbId, languageCode: navigator.language, requestOnBehalf: userId, qualityPathOverride: undefined, rootFolderOverride: undefined }).toPromise();
        if (result.result) {
            this.movie.requested = true;
            this.movie.requestId = result.requestId;
            this.movieRequest = await this.requestService.getMovieRequest(this.movie.requestId);
            this.messageService.send(this.translate.instant("Requests.RequestAddedSuccessfully", { title: this.movie.title }), "Ok");
        } else {
            this.messageService.send(result.errorMessage, "Ok");
        }
    }
    }

    public openDialog() {
        this.dialog.open(YoutubeTrailerComponent, {
            width: '560px',
            data: this.movie.videos.results[0].key
        });
    }

    public async deny() {
        const dialogRef = this.dialog.open(DenyDialogComponent, {
            width: '250px',
            data: { requestId: this.movieRequest.id, requestType: RequestType.movie }
        });

        dialogRef.afterClosed().subscribe(result => {
            this.movieRequest.denied = result.denied;
            this.movieRequest.deniedReason = result.reason;
        });
    }

    public async issue() {
        let provider = this.movie.id.toString();
        if (this.movie.imdbId) {
            provider = this.movie.imdbId;
        }
        const dialogRef = this.dialog.open(NewIssueComponent, {
            width: '500px',
            data: { requestId: this.movieRequest ? this.movieRequest.id : null, requestType: RequestType.movie, providerId: provider, title: this.movie.title }
        });
    }

    public async approve() {
        this.movie.approved = true;
        const result = await this.requestService.approveMovie({ id: this.movieRequest.id }).toPromise();
        if (result.result) {
            this.messageService.send(this.translate.instant("Requests.SuccessfullyApproved"), "Ok");
        } else {
            this.movie.approved = false;
            this.messageService.send(result.errorMessage, "Ok");
        }
    }

    public async markAvailable() {
        const result = await this.requestService.markMovieAvailable({ id: this.movieRequest.id }).toPromise();
        if (result.result) {
            this.movie.available = true;
            this.messageService.send(this.translate.instant("Requests.NowAvailable"), "Ok");
        } else {
            this.messageService.send(result.errorMessage, "Ok");
        }
    }


    public async markUnavailable() {
        const result = await this.requestService.markMovieUnavailable({ id: this.movieRequest.id }).toPromise();
        if (result.result) {
            this.movie.available = false;
            this.messageService.send(this.translate.instant("Requests.NowUnavailable"), "Ok");
        } else {
            this.messageService.send(result.errorMessage, "Ok");
        }
    }

    public setAdvancedOptions(data: IAdvancedData) {
        this.advancedOptions = data;
        if (data.rootFolderId) {
            this.movieRequest.qualityOverrideTitle = data.profiles.filter(x => x.id == data.profileId)[0].name;
        }
        if (data.profileId) {
            this.movieRequest.rootPathOverrideTitle = data.rootFolders.filter(x => x.id == data.rootFolderId)[0].path;
        }
    }

    public async openAdvancedOptions() {
        const dialog = this.dialog.open(MovieAdvancedOptionsComponent, { width: "700px", data: <IAdvancedData>{ movieRequest: this.movieRequest }, panelClass: 'modal-panel' })
        await dialog.afterClosed().subscribe(async result => {
            if (result) {
                result.rootFolder = result.rootFolders.filter(f => f.id === +result.rootFolderId)[0];
                result.profile = result.profiles.filter(f => f.id === +result.profileId)[0];
                await this.requestService2.updateMovieAdvancedOptions({ qualityOverride: result.profileId, rootPathOverride: result.rootFolderId, languageProfile: 0, requestId: this.movieRequest.id }).toPromise();
                this.setAdvancedOptions(result);
            }
        });
    }

    public reProcessRequest() {
        this.requestService2.reprocessRequest(this.movieRequest.id, RequestType.movie).subscribe(result => {
            if (result.result) {
                this.messageService.send(result.message ? result.message : this.translate.instant("Requests.SuccessfullyReprocessed"), "Ok");
            } else {
                this.messageService.send(result.errorMessage, "Ok");
            }
        });
    }

    private loadBanner() {
        this.imageService.getMovieBanner(this.theMovidDbId.toString()).subscribe(x => {
            if (!this.movie.backdropPath) {
            this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(" + x + ")");
            } else {
                this.movie.background = this.sanitizer.bypassSecurityTrustStyle
                ("url(https://image.tmdb.org/t/p/original/" + this.movie.backdropPath + ")");
            }
        });
    }

    private loadAdvancedInfo() {
        const profile = this.radarrService.getQualityProfilesFromSettings();
        const folders = this.radarrService.getRootFoldersFromSettings();

        forkJoin([profile, folders]).subscribe(x => {
            const radarrProfiles = x[0];
            const radarrRootFolders = x[1];

            const profile = radarrProfiles.filter((p) => {
                return p.id === this.movieRequest.qualityOverride;
            });
            if (profile.length > 0) {
                this.movieRequest.qualityOverrideTitle = profile[0].name;
            }

            const path = radarrRootFolders.filter((folder) => {
                return folder.id === this.movieRequest.rootPathOverride;
            });
            if (path.length > 0) {
                this.movieRequest.rootPathOverrideTitle = path[0].path;
            }

        });
    }
}
