import {
    CouchPotatoService,
    EmbyService,
    FileDownloadService,
    IssuesService,
    JellyfinService,
    JobService,
    LidarrService,
    MobileService,
    NotificationMessageService,
    PlexService,
    RadarrService,
    RequestRetryService,
    SonarrService,
    SystemService,
    TesterService,
    TheMovieDbService,
    ValidationService
} from "../services";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";

import { AboutComponent } from "./about/about.component";
import { AuthGuard } from "../auth/auth.guard";
import { AuthService } from "../auth/auth.service";
import { AuthenticationComponent } from "./authentication/authentication.component";
import {AutoCompleteModule} from "primeng/autocomplete";
import {CalendarModule} from "primeng/calendar";
import { ClipboardModule } from "ngx-clipboard";
import { CloudMobileComponent } from "./notifications/cloudmobile.coponent";
import { CloudMobileService } from "../services/cloudmobile.service";
import { CommonModule } from "@angular/common";
import { CouchPotatoComponent } from "./couchpotato/couchpotato.component";
import { CustomizationComponent } from "./customization/customization.component";
import {DialogModule} from "primeng/dialog";
import { DiscordComponent } from "./notifications/discord.component";
import { DogNzbComponent } from "./dognzb/dognzb.component";
import { EmailNotificationComponent } from "./notifications/emailnotification.component";
import { EmbyComponent } from "./emby/emby.component";
import { FailedRequestsComponent } from "./failedrequests/failedrequests.component";
import { FeaturesComponent } from "./features/features.component";
import { GotifyComponent } from "./notifications/gotify.component";
import { HubService } from "../services/hub.service";
import {InputSwitchModule} from "primeng/inputswitch";
import {InputTextModule} from "primeng/inputtext";
import { IssuesComponent } from "./issues/issues.component";
import { JellyfinComponent } from "./jellyfin/jellyfin.component";
import { JobsComponent } from "./jobs/jobs.component";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { LidarrComponent } from "./lidarr/lidarr.component";
import { LogsComponent } from "./logs/logs.component";
import { MassEmailComponent } from "./massemail/massemail.component";
import { MatDialogModule } from "@angular/material/dialog";
import { MatMenuModule } from "@angular/material/menu";
import { MattermostComponent } from "./notifications/mattermost.component";
import {MenuModule} from "primeng/menu";
import { MobileComponent } from "./notifications/mobile.component";
import { NewsletterComponent } from "./notifications/newsletter.component";
import { NgModule } from "@angular/core";
import { NotificationTemplate } from "./notifications/notificationtemplate.component";
import { OmbiComponent } from "./ombi/ombi.component";
import { PipeModule } from "../pipes/pipe.module";
import { PlexComponent } from "./plex/plex.component";
import { PushbulletComponent } from "./notifications/pushbullet.component";
import { PushoverComponent } from "./notifications/pushover.component";
import { RadarrComponent } from "./radarr/radarr.component";
import { RadarrFormComponent } from "./radarr/components/radarr-form.component";
import {RadioButtonModule} from "primeng/radiobutton";
import { SettingsMenuComponent } from "./settingsmenu.component";
import { SharedModule } from "../shared/shared.module";
import { SickRageComponent } from "./sickrage/sickrage.component";
import { SlackComponent } from "./notifications/slack.component";
import { SonarrComponent } from "./sonarr/sonarr.component";
import { TelegramComponent } from "./notifications/telegram.component";
import { TheMovieDbComponent } from "./themoviedb/themoviedb.component";
import {TooltipModule} from "primeng/tooltip";
import { TwilioComponent } from "./notifications/twilio/twilio.component";
import { UpdateComponent } from "./update/update.component";
import { UpdateDialogComponent } from "./about/update-dialog.component";
import { UpdateService } from "../services/update.service";
import { UserManagementComponent } from "./usermanagement/usermanagement.component";
import { VoteComponent } from "./vote/vote.component";
import { WebhookComponent } from "./notifications/webhook.component";
import { WhatsAppComponent } from "./notifications/twilio/whatsapp.component";
import { WikiComponent } from "./wiki.component";
import { PlexWatchlistComponent } from "./plex/components/watchlist/plex-watchlist.component";
import { PlexFormComponent } from "./plex/components/plex-form/plex-form.component";
import { PlexFormFieldComponent } from "./plex/components/form-field/plex-form-field.component";

const routes: Routes = [
    { path: "Ombi", component: OmbiComponent, canActivate: [AuthGuard] },
    { path: "About", component: AboutComponent, canActivate: [AuthGuard] },
    { path: "Plex", component: PlexComponent, canActivate: [AuthGuard] },
    { path: "Emby", component: EmbyComponent, canActivate: [AuthGuard] },
    { path: "Jellyfin", component: JellyfinComponent, canActivate: [AuthGuard] },
    { path: "Sonarr", component: SonarrComponent, canActivate: [AuthGuard] },
    { path: "Radarr", component: RadarrComponent, canActivate: [AuthGuard] },
    { path: "LandingPage", component: LandingPageComponent, canActivate: [AuthGuard] },
    { path: "Customization", component: CustomizationComponent, canActivate: [AuthGuard] },
    { path: "Email", component: EmailNotificationComponent, canActivate: [AuthGuard] },
    { path: "Discord", component: DiscordComponent, canActivate: [AuthGuard] },
    { path: "Slack", component: SlackComponent, canActivate: [AuthGuard] },
    { path: "Pushover", component: PushoverComponent, canActivate: [AuthGuard] },
    { path: "Pushbullet", component: PushbulletComponent, canActivate: [AuthGuard] },
    { path: "Gotify", component: GotifyComponent, canActivate: [AuthGuard] },
    { path: "Webhook", component: WebhookComponent, canActivate: [AuthGuard] },
    { path: "Mattermost", component: MattermostComponent, canActivate: [AuthGuard] },
    { path: "Twilio", component: TwilioComponent, canActivate: [AuthGuard] },
    { path: "UserManagement", component: UserManagementComponent, canActivate: [AuthGuard] },
    { path: "Update", component: UpdateComponent, canActivate: [AuthGuard] },
    { path: "CouchPotato", component: CouchPotatoComponent, canActivate: [AuthGuard] },
    { path: "DogNzb", component: DogNzbComponent, canActivate: [AuthGuard] },
    { path: "Telegram", component: TelegramComponent, canActivate: [AuthGuard] },
    { path: "Jobs", component: JobsComponent, canActivate: [AuthGuard] },
    { path: "SickRage", component: SickRageComponent, canActivate: [AuthGuard] },
    { path: "Issues", component: IssuesComponent, canActivate: [AuthGuard] },
    { path: "Authentication", component: AuthenticationComponent, canActivate: [AuthGuard] },
    { path: "Mobile", component: MobileComponent, canActivate: [AuthGuard] },
    { path: "MassEmail", component: MassEmailComponent, canActivate: [AuthGuard] },
    { path: "Newsletter", component: NewsletterComponent, canActivate: [AuthGuard] },
    { path: "Lidarr", component: LidarrComponent, canActivate: [AuthGuard] },
    { path: "Vote", component: VoteComponent, canActivate: [AuthGuard] },
    { path: "TheMovieDb", component: TheMovieDbComponent, canActivate: [AuthGuard] },
    { path: "FailedRequests", component: FailedRequestsComponent, canActivate: [AuthGuard] },
    { path: "Logs", component: LogsComponent, canActivate: [AuthGuard] },
    { path: "CloudMobile", component: CloudMobileComponent, canActivate: [AuthGuard] },
    { path: "Features", component: FeaturesComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(routes),
        MenuModule,
        InputSwitchModule,
        InputTextModule,
        TooltipModule,
        AutoCompleteModule,
        CalendarModule,
        // TagInputModule,
        ClipboardModule,
        PipeModule,
        RadioButtonModule,
        DialogModule,
        SharedModule,
        MatMenuModule,
        MatDialogModule
    ],
    declarations: [
        SettingsMenuComponent,
        OmbiComponent,
        PlexComponent,
        EmbyComponent,
        JellyfinComponent,
        JobsComponent,
        LandingPageComponent,
        CustomizationComponent,
        DiscordComponent,
        SonarrComponent,
        SlackComponent,
        RadarrComponent,
        RadarrFormComponent,
        EmailNotificationComponent,
        NotificationTemplate,
        PushoverComponent,
        MattermostComponent,
        PushbulletComponent,
        GotifyComponent,
        WebhookComponent,
        UserManagementComponent,
        UpdateComponent,
        AboutComponent,
        WikiComponent,
        CouchPotatoComponent,
        DogNzbComponent,
        SickRageComponent,
        TelegramComponent,
        IssuesComponent,
        AuthenticationComponent,
        MobileComponent,
        MassEmailComponent,
        NewsletterComponent,
        LidarrComponent,
        VoteComponent,
        TheMovieDbComponent,
        FailedRequestsComponent,
        LogsComponent,
        TwilioComponent,
        WhatsAppComponent,
        FeaturesComponent,
        CloudMobileComponent,
        UpdateDialogComponent,
        PlexWatchlistComponent,
        PlexFormComponent,
        PlexFormFieldComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        SonarrService,
        AuthService,
        RadarrService,
        AuthGuard,
        ValidationService,
        TesterService,
        JobService,
        CouchPotatoService,
        IssuesService,
        PlexService,
        EmbyService,
        JellyfinService,
        MobileService,
        NotificationMessageService,
        LidarrService,
        RequestRetryService,
        HubService,
        SystemService,
        FileDownloadService,
        TheMovieDbService,
        CloudMobileService,
        UpdateService,
    ],

})
export class SettingsModule { }
