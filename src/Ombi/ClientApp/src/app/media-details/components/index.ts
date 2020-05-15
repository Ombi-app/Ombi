import { MovieDetailsComponent } from "./movie/movie-details.component";
import { YoutubeTrailerComponent } from "./shared/youtube-trailer.component";
import { TvDetailsComponent } from "./tv/tv-details.component";
import { MovieInformationPanelComponent } from "./movie/panels/movie-information-panel.component";
import { TvInformationPanelComponent } from "./tv/panels/tv-information-panel/tv-information-panel.component";
import { TopBannerComponent } from "./shared/top-banner/top-banner.component";
import { SocialIconsComponent } from "./shared/social-icons/social-icons.component";
import { MediaPosterComponent } from "./shared/media-poster/media-poster.component";
import { CastCarouselComponent } from "./shared/cast-carousel/cast-carousel.component";
import { DenyDialogComponent } from "./shared/deny-dialog/deny-dialog.component";
import { TvRequestsPanelComponent } from "./tv/panels/tv-requests/tv-requests-panel.component";
import { MovieAdminPanelComponent } from "./movie/panels/movie-admin-panel/movie-admin-panel.component";
import { MovieAdvancedOptionsComponent } from "./movie/panels/movie-advanced-options/movie-advanced-options.component";
import { SearchService, RequestService, RadarrService, IssuesService } from "../../services";
import { RequestServiceV2 } from "../../services/requestV2.service";
import { NewIssueComponent } from "./shared/new-issue/new-issue.component";
import { ArtistDetailsComponent } from "./artist/artist-details.component";
import { ArtistInformationPanel } from "./artist/panels/artist-information-panel/artist-information-panel.component";
import { ArtistReleasePanel } from "./artist/panels/artist-release-panel/artist-release-panel.component";
import { IssuesPanelComponent } from "./shared/issues-panel/issues-panel.component";

export const components: any[] = [
    MovieDetailsComponent,
    YoutubeTrailerComponent,
    TvDetailsComponent,
    MovieInformationPanelComponent,
    TvInformationPanelComponent,
    TopBannerComponent,
    SocialIconsComponent,
    MediaPosterComponent,
    CastCarouselComponent,
    DenyDialogComponent,
    TvRequestsPanelComponent,
    MovieAdminPanelComponent,
    MovieAdvancedOptionsComponent,
    NewIssueComponent,
    ArtistDetailsComponent,
    ArtistInformationPanel,
    ArtistReleasePanel,
    IssuesPanelComponent,
];

export const entryComponents: any[] = [
    YoutubeTrailerComponent,
    DenyDialogComponent,
    MovieAdvancedOptionsComponent,
    NewIssueComponent,
];

export const providers: any[] = [
    SearchService,
    RequestService,
    RadarrService,
    RequestServiceV2,
    IssuesService,
];
