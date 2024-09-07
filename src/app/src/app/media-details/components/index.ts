import { IssuesService, RadarrService, RequestService, SearchService, SonarrService } from "../../services";

import { ArtistDetailsComponent } from "./artist/artist-details.component";
import { ArtistInformationPanel } from "./artist/panels/artist-information-panel/artist-information-panel.component";
import { ArtistReleasePanel } from "./artist/panels/artist-release-panel/artist-release-panel.component";
import { CastCarouselComponent } from "./shared/cast-carousel/cast-carousel.component";
import { CrewCarouselComponent } from "./shared/crew-carousel/crew-carousel.component";
import { DenyDialogComponent } from "./shared/deny-dialog/deny-dialog.component";
import { IssuesPanelComponent } from "./shared/issues-panel/issues-panel.component";
import { MediaPosterComponent } from "./shared/media-poster/media-poster.component";
import { MovieAdvancedOptionsComponent } from "./movie/panels/movie-advanced-options/movie-advanced-options.component";
import { MovieDetailsComponent } from "./movie/movie-details.component";
import { MovieInformationPanelComponent } from "./movie/panels/movie-information-panel.component";
import { NewIssueComponent } from "./shared/new-issue/new-issue.component";
import { RequestBehalfComponent } from "./shared/request-behalf/request-behalf.component";
import { RequestServiceV2 } from "../../services/requestV2.service";
import { SocialIconsComponent } from "./shared/social-icons/social-icons.component";
import { TopBannerComponent } from "./shared/top-banner/top-banner.component";
import { TvAdvancedOptionsComponent } from "./tv/panels/tv-advanced-options/tv-advanced-options.component";
import { TvDetailsComponent } from "./tv/tv-details.component";
import { TvInformationPanelComponent } from "./tv/panels/tv-information-panel/tv-information-panel.component";
import { TvRequestGridComponent } from "./tv/panels/tv-request-grid/tv-request-grid.component";
import { TvRequestsPanelComponent } from "./tv/panels/tv-requests/tv-requests-panel.component";
import { YoutubeTrailerComponent } from "./shared/youtube-trailer.component";

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
    CrewCarouselComponent,
    DenyDialogComponent,
    TvRequestsPanelComponent,
    MovieAdvancedOptionsComponent,
    TvAdvancedOptionsComponent,
    NewIssueComponent,
    ArtistDetailsComponent,
    ArtistInformationPanel,
    ArtistReleasePanel,
    RequestBehalfComponent,
    IssuesPanelComponent,
    TvRequestGridComponent,
];

export const providers: any[] = [
    SearchService,
    RequestService,
    RadarrService,
    RequestServiceV2,
    IssuesService,
    SonarrService,
];
