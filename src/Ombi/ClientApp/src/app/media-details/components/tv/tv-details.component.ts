import { Component, ViewEncapsulation, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatDialogModule } from "@angular/material/dialog";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatTabsModule } from "@angular/material/tabs";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatCardModule } from "@angular/material/card";
import { MatExpansionModule } from "@angular/material/expansion";
import { TranslateModule } from "@ngx-translate/core";
import { CarouselModule } from "primeng/carousel";
import { SkeletonModule } from "primeng/skeleton";
import { SearchV2Service, MessageService, RequestService, SonarrService, SettingsStateService, MediaCleanupService } from "../../../services";
import { ActivatedRoute } from "@angular/router";
import { DomSanitizer } from "@angular/platform-browser";
import { ISearchTvResultV2 } from "../../../interfaces/ISearchTvResultV2";
import { MatDialog } from "@angular/material/dialog";
import { YoutubeTrailerComponent } from "../shared/youtube-trailer.component";
import { IAdvancedData, IChildRequests, ITvRequests, RequestType, IMediaCleanupActionResult, IMediaCleanupItem, IMediaCleanupOverview, CommunityCleanupMode, MediaCleanupStatus, MediaCleanupVoteType, OwnRequestRemovalMode } from "../../../interfaces";
import { AuthService } from "../../../auth/auth.service";
import { NewIssueComponent } from "../shared/new-issue/new-issue.component";
import { TvAdvancedOptionsComponent } from "./panels/tv-advanced-options/tv-advanced-options.component";
import { RequestServiceV2 } from "../../../services/requestV2.service";
import { firstValueFrom, forkJoin, Observable } from "rxjs";
import { SonarrFacade } from "app/state/sonarr";
import { TopBannerComponent } from "../shared/top-banner/top-banner.component";
import { SocialIconsComponent } from "../shared/social-icons/social-icons.component";
import { MediaPosterComponent } from "../shared/media-poster/media-poster.component";
import { CastCarouselComponent } from "../shared/cast-carousel/cast-carousel.component";
import { CrewCarouselComponent } from "../shared/crew-carousel/crew-carousel.component";
import { TvInformationPanelComponent } from "./panels/tv-information-panel/tv-information-panel.component";
import { TvRequestsPanelComponent } from "./panels/tv-requests/tv-requests-panel.component";
import { TvRequestGridComponent } from "./panels/tv-request-grid/tv-request-grid.component";
import { IssuesPanelComponent } from "../shared/issues-panel/issues-panel.component";
import { ImageComponent } from "../../../components";
import { OmbiDatePipe } from "../../../pipes/OmbiDatePipe";
import { TranslateStatusPipe } from "../../../pipes/TranslateStatus";

@Component({
    standalone: true,
    templateUrl: "./tv-details.component.html",
    styleUrls: ["../../media-details.component.scss"],
    encapsulation: ViewEncapsulation.None,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatDialogModule,
        MatProgressSpinnerModule,
        MatTabsModule,
        MatTooltipModule,
        MatCardModule,
        MatExpansionModule,
        TranslateModule,
        CarouselModule,
        SkeletonModule,
        TopBannerComponent,
        SocialIconsComponent,
        MediaPosterComponent,
        CastCarouselComponent,
        TvInformationPanelComponent,
        TvRequestsPanelComponent,
        TvRequestGridComponent,
        IssuesPanelComponent,
        OmbiDatePipe,
        TranslateStatusPipe
    ]
})
export class TvDetailsComponent implements OnInit {

    public tv: ISearchTvResultV2;
    public tvRequest: IChildRequests[];
    public showRequest: ITvRequests;
    public fromSearch: boolean;
    public isAdmin: boolean;
    public manageOwnRequests: boolean;
    public advancedOptions: IAdvancedData;
    public showAdvanced: boolean; // Set on the UI
    public canSelectQualityProfile = false;
    public requestType = RequestType.tvShow;
    public issuesEnabled: boolean;
    public cleanupOverview?: IMediaCleanupOverview;
    public cleanupItem?: IMediaCleanupItem;
    public cleanupBusy = false;
    public metadataUnavailable = false;
    public readonly MediaCleanupVoteType = MediaCleanupVoteType;

    private tvdbId: number;

    constructor(private searchService: SearchV2Service, private route: ActivatedRoute,
        private sanitizer: DomSanitizer,
        public dialog: MatDialog,
        public messageService: MessageService,
        private requestService: RequestService,
        private requestService2: RequestServiceV2,
        private auth: AuthService,
        private sonarrService: SonarrService,
        private sonarrFacade: SonarrFacade,
        private settingsState: SettingsStateService,
        private cleanupService?: MediaCleanupService) {
        this.route.params.subscribe((params: any) => {
            this.tvdbId = params.tvdbId;
            this.fromSearch = params.search;
        });
    }

    public async ngOnInit() {
        await this.load();
        if (this.tv) {
            this.checkPoster();
        }
    }

    public async load() {

        this.issuesEnabled = this.settingsState.getIssue();
        this.isAdmin = this.auth.hasRole("admin") || this.auth.hasRole("poweruser");
        this.canSelectQualityProfile = !this.isAdmin && this.auth.hasRole("SelectQualityProfile");
        this.manageOwnRequests = this.auth.hasRole('ManageOwnRequests');

        if (this.isAdmin) {
            this.showAdvanced = this.sonarrFacade.isEnabled();
        }

        // if (this.fromSearch) {
        //     this.tv = await this.searchService.getTvInfoWithMovieDbId(this.tvdbId);
        //     this.tvdbId = this.tv.id;
        // } else {
        this.metadataUnavailable = false;
        try {
            this.tv = await this.searchService.getTvInfo(this.tvdbId);
        } catch {
            this.metadataUnavailable = true;
            return;
        }
        // }

        if (!this.tv) {
            this.metadataUnavailable = true;
            return;
        }

        if (this.tv.requestId) {
            this.tvRequest = await this.requestService.getChildRequests(this.tv.requestId).toPromise();
            this.showRequest = this.tvRequest.length > 0 ? this.tvRequest[0].parentRequest : undefined;
            this.loadAdvancedInfo();
        }

        void this.loadCleanupContext();

        // const tvBanner = await this.imageService.getTvBanner(this.tvdbId).toPromise();
        if (this.tv.banner && this.tv.banner !== null && this.tv.banner !== undefined) {
            this.tv.background = this.sanitizer.bypassSecurityTrustStyle("url(https://image.tmdb.org/t/p/original" + this.tv.banner + ")");
        } else {
            this.tv.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(rgba(0,0,0,.5), rgba(0,0,0,.5))");
        }
    }

    public request() {
        const grid = document.getElementById("requests-grid");
        if (grid) {
            grid.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    public cleanupOwnActionText(): string {
        return this.cleanupOverview?.settings?.ownRequestRemoval === OwnRequestRemovalMode.ImmediateDeletion
            ? "Remove Media"
            : "Request Removal";
    }

    public showCommunityNominationAction(): boolean {
        return !!this.cleanupOverview &&
            !!this.cleanupItem &&
            !this.cleanupItem.cleanup &&
            this.cleanupOverview.canVote &&
            this.cleanupOverview.settings.communityCleanup !== CommunityCleanupMode.Off;
    }

    public cleanupNominationActionText(): string {
        if (this.cleanupItem?.canNominate) {
            return 'Nominate for Cleanup';
        }

        if (!this.cleanupItem?.communityAgeEligible) {
            const minimumDays = this.cleanupOverview?.settings?.minimumMediaAgeDays ?? 0;
            const availableSince = this.cleanupItem?.availableSince ? new Date(this.cleanupItem.availableSince) : undefined;
            if (minimumDays > 0 && availableSince && !Number.isNaN(availableSince.getTime())) {
                const eligibleAt = new Date(availableSince);
                eligibleAt.setDate(eligibleAt.getDate() + minimumDays);
                const remainingDays = Math.max(1, Math.ceil((eligibleAt.getTime() - Date.now()) / 86400000));
                return remainingDays === 1 ? 'Cleanup eligible tomorrow' : `Cleanup eligible in ${remainingDays} days`;
            }
            return minimumDays > 0 ? `Cleanup requires ${minimumDays} days available` : 'Not eligible for cleanup';
        }

        return 'Not eligible for cleanup';
    }

    public cleanupNominationTooltip(): string {
        if (this.cleanupItem?.canNominate) {
            return 'Start a community cleanup vote for this title.';
        }
        if (!this.cleanupItem?.communityAgeEligible) {
            const minimumDays = this.cleanupOverview?.settings?.minimumMediaAgeDays ?? 0;
            return minimumDays > 0
                ? `Community cleanup requires media to have been available for at least ${minimumDays} days.`
                : 'This title is not yet eligible for community cleanup.';
        }
        return 'This title is not currently eligible for community cleanup.';
    }

    public cleanupStatusText(status: MediaCleanupStatus): string {
        switch (status) {
            case MediaCleanupStatus.Voting: return "Cleanup vote active";
            case MediaCleanupStatus.PendingAdminApproval: return "Cleanup awaiting approval";
            case MediaCleanupStatus.ScheduledForDeletion: return "Scheduled for removal";
            case MediaCleanupStatus.Completed: return "Media removed";
            case MediaCleanupStatus.Rejected: return "Cleanup rejected";
            case MediaCleanupStatus.Failed: return "Cleanup failed";
            case MediaCleanupStatus.Cancelled: return "Cleanup cancelled";
            default: return "Cleanup active";
        }
    }

    public async requestMediaRemoval(): Promise<void> {
        if (!this.cleanupService || !this.cleanupItem || !this.cleanupOverview) {
            return;
        }
        const immediate = this.cleanupOverview.settings.ownRequestRemoval === OwnRequestRemovalMode.ImmediateDeletion;
        if (immediate && !window.confirm(`Permanently remove ${this.tv.title} from the library? Media files will be deleted if that option is enabled.`)) {
            return;
        }
        const result = await this.executeCleanup(this.cleanupService.requestOwnRemoval(RequestType.tvShow, this.cleanupItem.requestId));
        if (immediate && result?.result) {
            // Keep the already-open details page in sync with the successful server-side
            // deletion so Requested/Available does not linger until a hard refresh.
            this.tv.requested = false;
            this.tv.approved = false;
            this.tv.available = false;
            this.tv.fullyAvailable = false;
            this.tv.partlyAvailable = false;
            this.tv.requestId = 0;
            this.tv.plexUrl = '';
            this.tv.embyUrl = '';
            this.tv.jellyfinUrl = '';
            for (const season of this.tv.seasonRequests ?? []) {
                season.seasonAvailable = false;
                for (const episode of season.episodes ?? []) {
                    episode.requested = false;
                    episode.approved = false;
                    episode.available = false;
                }
            }
        }
    }

    public async nominateForCleanup(): Promise<void> {
        if (!this.cleanupService || !this.cleanupItem) {
            return;
        }
        await this.executeCleanup(this.cleanupService.nominate(RequestType.tvShow, this.cleanupItem.requestId));
    }

    public async voteOnCleanup(vote: MediaCleanupVoteType): Promise<void> {
        if (!this.cleanupService || !this.cleanupItem?.cleanup) {
            return;
        }
        await this.executeCleanup(this.cleanupService.vote(this.cleanupItem.cleanup.id, vote));
    }

    private async loadCleanupContext(): Promise<void> {
        if (!this.cleanupService || !this.tv?.fullyAvailable || !this.tv?.id) {
            this.cleanupOverview = undefined;
            this.cleanupItem = undefined;
            return;
        }
        try {
            this.cleanupOverview = await firstValueFrom(this.cleanupService.getOverviewForMedia(RequestType.tvShow, this.tv.id));
            this.cleanupItem = this.cleanupOverview.items.find(x => x.requestType === RequestType.tvShow);
        } catch {
            // Cleanup actions are optional on the media details page. Do not fail media loading if cleanup is unavailable.
            this.cleanupOverview = undefined;
            this.cleanupItem = undefined;
        }
    }

    private async executeCleanup(request: Observable<IMediaCleanupActionResult>): Promise<IMediaCleanupActionResult | undefined> {
        this.cleanupBusy = true;
        try {
            const result = await firstValueFrom(request);
            this.messageService.send(result.message);
            await this.loadCleanupContext();
            return result;
        } catch (error: any) {
            this.messageService.send(error?.error?.message ?? "Media cleanup action failed.");
            return undefined;
        } finally {
            this.cleanupBusy = false;
        }
    }

    public async issue() {
        const dialogRef = this.dialog.open(NewIssueComponent, {
            width: '500px',
            data: { requestId: this.tvRequest ? this.tv.requestId : null, requestType: RequestType.tvShow, providerId: this.tv.id, title: this.tv.title, posterPath: this.tv.images.original }
        });
    }

    public openDialog() {
        let trailerLink = this.tv.trailer;

        this.dialog.open(YoutubeTrailerComponent, {
            width: '560px',
            data: trailerLink
        });
    }

    public async openAdvancedOptions() {
        const dialog = this.dialog.open(TvAdvancedOptionsComponent, { width: "700px", data: <IAdvancedData>{ tvRequest: this.showRequest }, panelClass: 'modal-panel' })
        await dialog.afterClosed().subscribe(async result => {
            if (result) {
                // get the name and ids
                result.rootFolder = result.rootFolders.filter(f => f.id === +result.rootFolderId)[0];
                result.profile = result.profiles.filter(f => f.id === +result.profileId)[0];
                result.language = result.languages.filter(x => x.id === +result.langaugeId)[0];
                await this.requestService2.updateTvAdvancedOptions({ qualityOverride: result.profileId, rootPathOverride: result.rootFolderId, languageProfile: result.languageId, requestId: this.showRequest.id }).toPromise();
                this.setAdvancedOptions(result);
            }
        });
    }

    public setAdvancedOptions(data: IAdvancedData) {
        this.advancedOptions = data;
        console.log(this.advancedOptions);
        if (data.rootFolderId) {
            this.showRequest.qualityOverrideTitle = data.profiles.filter(x => x.id == data.profileId)[0].name;
        }
        if (data.profileId) {
            this.showRequest.rootPathOverrideTitle =  data.rootFolders.filter(x => x.id == data.rootFolderId)[0].path;
        }
        if (data.languageId) {
            this.showRequest.languageOverrideTitle =  data.languages.filter(x => x.id == data.languageId)[0].name;
        }
    }

    public allEpisodesRequestedOrAvailable(): boolean {
      return this.tv.seasonRequests.every(e => e.episodes.every(x => x.available || x.approved || x.requested));
    }

    private checkPoster() {
      if (this.tv.images.original == null || this.tv.images.original === undefined) {
        this.tv.images.original = "../../../images/default_movie_poster.png";
      }
      else {
        this.tv.images.original = 'https://image.tmdb.org/t/p/w300/' + this.tv.images.original
      };
    }

    private loadAdvancedInfo() {
        const profile = this.sonarrService.getQualityProfilesWithoutSettings();
        const folders = this.sonarrService.getRootFoldersWithoutSettings();
        const languages = this.sonarrService.getV3LanguageProfilesWithoutSettings();

        forkJoin([profile, folders, languages]).subscribe(x => {
            const sonarrProfiles = x[0];
            const sonarrRootFolders = x[1];
            const languageProfiles = x[2];

            const profile = sonarrProfiles.filter((p) => {
                return p.id === this.showRequest.qualityOverride;
            });
            if (profile.length > 0) {
                this.showRequest.qualityOverrideTitle = profile[0].name;
            }

            const path = sonarrRootFolders.filter((folder) => {
                return folder.id === this.showRequest.rootFolder;
            });
            if (path.length > 0) {
                this.showRequest.rootPathOverrideTitle = path[0].path;
            }

            const lang = languageProfiles.filter((folder) => {
                return folder.id === this.showRequest.languageProfile;
            });
            if (lang.length > 0) {
                this.showRequest.languageOverrideTitle = lang[0].name;
            }

        });
    }
}
