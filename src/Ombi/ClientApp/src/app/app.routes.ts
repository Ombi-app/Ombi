import { Routes } from "@angular/router";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { LoginComponent } from "./login/login.component";
import { LoginOAuthComponent } from "./login/loginoauth.component";
import { CustomPageComponent } from "./custompage/custompage.component";
import { ResetPasswordComponent } from "./login/resetpassword.component";
import { TokenResetPasswordComponent } from "./login/tokenresetpassword.component";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { CookieComponent } from "./auth/cookie.component";
import { DiscoverComponent } from "./discover/components/discover/discover.component";
import { VoteComponent } from "./vote/vote.component";
import { SettingsComponent } from "./settings/settings.component";
import { MovieDetailsComponent } from "./media-details/components/movie/movie-details.component";
import { TvDetailsComponent } from "./media-details/components/tv/tv-details.component";
import { ArtistDetailsComponent } from "./media-details/components/artist/artist-details.component";
import { RequestsListComponent } from "./requests-list/components/requests-list.component";
import { AboutComponent } from "./settings/about/about.component";
import { OmbiComponent } from "./settings/ombi/ombi.component";
import { PlexComponent } from "./settings/plex/plex.component";
import { PlexServerDialogComponent } from "./settings/plex/components/plex-server-dialog/plex-server-dialog.component";
import { EmbyComponent } from "./settings/emby/emby.component";
import { JellyfinComponent } from "./settings/jellyfin/jellyfin.component";
import { SonarrComponent } from "./settings/sonarr/sonarr.component";
import { RadarrComponent } from "./settings/radarr/radarr.component";
import { RadarrFormComponent } from "./settings/radarr/components/radarr-form.component";
import { LandingPageComponent as SettingsLandingPageComponent } from "./settings/landingpage/landingpage.component";
import { CustomizationComponent } from "./settings/customization/customization.component";
import { EmailNotificationComponent } from "./settings/notifications/emailnotification.component";
import { UserManagementComponent as SettingsUserManagementComponent } from "./settings/usermanagement/usermanagement.component";
    import { CouchPotatoComponent } from "./settings/couchpotato/couchpotato.component";
import { JobsComponent } from "./settings/jobs/jobs.component";
import { SickRageComponent } from "./settings/sickrage/sickrage.component";
import { IssuesComponent as SettingsIssuesComponent } from "./settings/issues/issues.component";
import { AuthenticationComponent } from "./settings/authentication/authentication.component";
import { MassEmailComponent } from "./settings/massemail/massemail.component";
import { LidarrComponent } from "./settings/lidarr/lidarr.component";
import { VoteComponent as SettingsVoteComponent } from "./settings/vote/vote.component";
import { TheMovieDbComponent } from "./settings/themoviedb/themoviedb.component";
import { FailedRequestsComponent } from "./settings/failedrequests/failedrequests.component";
import { LogsComponent } from "./settings/logs/logs.component";
import { CloudMobileComponent } from "./settings/notifications/cloudmobile.coponent";
import { FeaturesComponent } from "./settings/features/features.component";
import { AuthGuard } from "./auth/auth.guard";
import { NewsletterComponent } from "./settings/notifications/newsletter.component";
import { DiscordComponent } from "./settings/notifications/discord.component";
import { SlackComponent } from "./settings/notifications/slack.component";
import { PushbulletComponent } from "./settings/notifications/pushbullet.component";
import { PushoverComponent } from "./settings/notifications/pushover.component";
import { MattermostComponent } from "./settings/notifications/mattermost.component";
import { TelegramComponent } from "./settings/notifications/telegram.component";
import { GotifyComponent } from "./settings/notifications/gotify.component";
import { TwilioComponent } from "./settings/notifications/twilio/twilio.component";
import { WebhookComponent } from "./settings/notifications/webhook.component";
import { IssuesComponent } from "./issues/issues.component";
import { IssuesDetailsComponent } from "./issues/components/details/details.component";
import { WelcomeComponent } from "./wizard/welcome/welcome.component";
import { MediaServerComponent } from "./wizard/mediaserver/mediaserver.component";
import { PlexComponent as WizardPlexComponent } from "./wizard/plex/plex.component";
import { EmbyComponent as WizardEmbyComponent } from "./wizard/emby/emby.component";
import { JellyfinComponent as WizardJellyfinComponent } from "./wizard/jellyfin/jellyfin.component";
import { CreateAdminComponent } from "./wizard/createadmin/createadmin.component";
import { OmbiConfigComponent } from "./wizard/ombiconfig/ombiconfig.component";
import { DatabaseComponent } from "./wizard/database/database.component";
import { UserManagementComponent } from "./usermanagement/usermanagement.component";
import { UserManagementUserComponent } from "./usermanagement/usermanagement-user.component";
import { UserPreferenceComponent } from "./user-preferences/components/user-preference/user-preference.component";
import { UnsubscribeConfirmComponent } from "./unsubscribe/components/confirm-component/unsubscribe-confirm.component";
import { DiscoverCollectionsComponent } from "./discover/components/collections/discover-collections.component";
import { DiscoverActorComponent } from "./discover/components/actor/discover-actor.component";
import { DiscoverSearchResultsComponent } from "./discover/components/search-results/search-results.component";


export const routes: Routes = [
    { path: "*", component: PageNotFoundComponent },
    { path: "", redirectTo: "/discover", pathMatch: "full" },
    { path: "login", component: LoginComponent },
    { path: "Login/OAuth/:pin", component: LoginOAuthComponent },
    { path: "Custom", component: CustomPageComponent },
    { path: "login/:landing", component: LoginComponent },
    { path: "reset", component: ResetPasswordComponent },
    { path: "token", component: TokenResetPasswordComponent },
    { path: "landingpage", component: LandingPageComponent },
    { path: "auth/cookie", component: CookieComponent },
    { path: "discover", component: DiscoverComponent },
    { path: "discover/collection/:collectionId", component:     DiscoverCollectionsComponent },
    { path: "discover/actor/:actorId", component: DiscoverActorComponent },
    { path: "discover/:searchTerm", component: DiscoverSearchResultsComponent },
    { path: "discover/advanced/search", component: DiscoverSearchResultsComponent },
    { path: "vote", component: VoteComponent },
    { 
        path: "issues", 
        children: [
            { path: "", component: IssuesComponent, canActivate: [AuthGuard] },
            { path: ":providerId", component: IssuesDetailsComponent, canActivate: [AuthGuard] }
        ]
    },
    {
        path: "Settings",
        component: SettingsComponent,
        canActivate: [AuthGuard],
        children: [
            { path: "Ombi", component: OmbiComponent },
            { path: "About", component: AboutComponent },
            { path: "Discord", component: DiscordComponent },
            { path: "Newsletter", component: NewsletterComponent },
            { path: "Slack", component: SlackComponent },
            { path: "Pushbullet", component: PushbulletComponent },
            { path: "Pushover", component: PushoverComponent },
            { path: "Mattermost", component: MattermostComponent },
            { path: "Telegram", component: TelegramComponent },
            { path: "Gotify", component: GotifyComponent },
            { path: "Twilio", component: TwilioComponent },
            { path: "Webhook", component: WebhookComponent },
            { path: "Plex", component: PlexComponent },
            { path: "Emby", component: EmbyComponent },
            { path: "Jellyfin", component: JellyfinComponent },
            { path: "Sonarr", component: SonarrComponent },
            { path: "Radarr", component: RadarrComponent },
            { path: "LandingPage", component: SettingsLandingPageComponent },
            { path: "Customization", component: CustomizationComponent },
            { path: "Email", component: EmailNotificationComponent },
            { path: "UserManagement", component: SettingsUserManagementComponent },
            { path: "CouchPotato", component: CouchPotatoComponent },
            { path: "Jobs", component: JobsComponent },
            { path: "SickRage", component: SickRageComponent },
            { path: "Issues", component: SettingsIssuesComponent },
            { path: "Authentication", component: AuthenticationComponent },
            { path: "MassEmail", component: MassEmailComponent },
            { path: "Lidarr", component: LidarrComponent },
            { path: "Vote", component: SettingsVoteComponent },
            { path: "TheMovieDb", component: TheMovieDbComponent },
            { path: "FailedRequests", component: FailedRequestsComponent },
            { path: "Logs", component: LogsComponent },
            { path: "CloudMobile", component: CloudMobileComponent },
            { path: "Features", component: FeaturesComponent },
            { path: "", redirectTo: "About", pathMatch: "full" }
        ]
    },
    { 
        path: "Wizard", 
        children: [
            { path: "", component: WelcomeComponent },
            { path: "MediaServer", component: MediaServerComponent },
            { path: "Plex", component: WizardPlexComponent },
            { path: "Emby", component: WizardEmbyComponent },
            { path: "Jellyfin", component: WizardJellyfinComponent },
            { path: "CreateAdmin", component: CreateAdminComponent },
            { path: "OmbiConfig", component: OmbiConfigComponent }
        ]
    },
    { 
        path: "usermanagement", 
        children: [
            { path: "", component: UserManagementComponent, canActivate: [AuthGuard] },
            { path: "user", component: UserManagementUserComponent, canActivate: [AuthGuard] },
            { path: "user/:id", component: UserManagementUserComponent, canActivate: [AuthGuard] }
        ]
    },
    // { loadChildren: () => import("./requests/requests.module").then(m => m.RequestsModule), path: "requestsOld" },
    { path: "requests-list", component: RequestsListComponent, canActivate: [AuthGuard] },
    {
        path: "details",
        children: [
            { path: "movie/:movieDbId", component: MovieDetailsComponent, canActivate: [AuthGuard] },
            { path: "tv/:tvdbId/:search", component: TvDetailsComponent, canActivate: [AuthGuard] },
            { path: "tv/:tvdbId", component: TvDetailsComponent, canActivate: [AuthGuard] },
            { path: "artist/:artistId", component: ArtistDetailsComponent, canActivate: [AuthGuard] }
        
        ]
    },
    { path: "user-preferences", component: UserPreferenceComponent, canActivate: [AuthGuard] },
    { path: "unsubscribe/:id", component: UnsubscribeConfirmComponent },
];
