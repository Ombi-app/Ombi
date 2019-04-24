﻿import { MovieDetailsComponent } from "./movie/movie-details.component";
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
    MovieAdvancedOptionsComponent
];



export const entryComponents: any[] = [
    YoutubeTrailerComponent,
    DenyDialogComponent,
    MovieAdvancedOptionsComponent,
];
