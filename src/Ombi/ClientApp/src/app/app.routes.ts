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
    { path: "vote", component: VoteComponent },
    { loadChildren: () => import("./issues/issues.module").then(m => m.IssuesModule), path: "issues" },
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
    { loadChildren: () => import("./wizard/wizard.module").then(m => m.WizardModule), path: "Wizard" },
    { loadChildren: () => import("./usermanagement/usermanagement.module").then(m => m.UserManagementModule), path: "usermanagement" },
    // { loadChildren: () => import("./requests/requests.module").then(m => m.RequestsModule), path: "requestsOld" },
    { loadChildren: () => import("./requests-list/requests-list.module").then(m => m.RequestsListModule), path: "requests-list" },
    { loadChildren: () => import("./media-details/media-details.module").then(m => m.MediaDetailsModule), path: "details" },
    { loadChildren: () => import("./user-preferences/user-preferences.module").then(m => m.UserPreferencesModule), path: "user-preferences" },
    { loadChildren: () => import("./unsubscribe/unsubscribe.module").then(m => m.UnsubscribeModule), path: "unsubscribe" },
];
