import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { NgbAccordionModule, NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { ClipboardModule } from "ngx-clipboard/dist";

import { AuthGuard } from "../auth/auth.guard";
import { AuthModule } from "../auth/auth.module";
import { AuthService } from "../auth/auth.service";
import { CouchPotatoService, JobService, RadarrService, SonarrService, TesterService, ValidationService } from "../services";

import { PipeModule } from "../pipes/pipe.module";
import { AboutComponent } from "./about/about.component";
import { CouchPotatoComponent } from "./couchpotato/couchpotato.component";
import { CustomizationComponent } from "./customization/customization.component";
import { DogNzbComponent } from "./dognzb/dognzb.component";
import { EmbyComponent } from "./emby/emby.component";
import { JobsComponent } from "./jobs/jobs.component";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { DiscordComponent } from "./notifications/discord.component";
import { EmailNotificationComponent } from "./notifications/emailnotification.component";
import { MattermostComponent } from "./notifications/mattermost.component";
import { NotificationTemplate } from "./notifications/notificationtemplate.component";
import { PushbulletComponent } from "./notifications/pushbullet.component";
import { PushoverComponent } from "./notifications/pushover.component";
import { SlackComponent } from "./notifications/slack.component";
import { TelegramComponent } from "./notifications/telegram.component";
import { OmbiComponent } from "./ombi/ombi.component";
import { PlexComponent } from "./plex/plex.component";
import { RadarrComponent } from "./radarr/radarr.component";
import { SickRageComponent } from "./sickrage/sickrage.component";
import { SonarrComponent } from "./sonarr/sonarr.component";
import { UpdateComponent } from "./update/update.component";
import { UserManagementComponent } from "./usermanagement/usermanagement.component";
import { WikiComponent } from "./wiki.component";

import { SettingsMenuComponent } from "./settingsmenu.component";

import { AutoCompleteModule, CalendarModule, InputSwitchModule, InputTextModule, MenuModule, RadioButtonModule, TooltipModule } from "primeng/primeng";

const routes: Routes = [
    { path: "Settings/Ombi", component: OmbiComponent, canActivate: [AuthGuard] },
    { path: "Settings/About", component: AboutComponent, canActivate: [AuthGuard] },
    { path: "Settings/Plex", component: PlexComponent, canActivate: [AuthGuard] },
    { path: "Settings/Emby", component: EmbyComponent, canActivate: [AuthGuard] },
    { path: "Settings/Sonarr", component: SonarrComponent, canActivate: [AuthGuard] },
    { path: "Settings/Radarr", component: RadarrComponent, canActivate: [AuthGuard] },
    { path: "Settings/LandingPage", component: LandingPageComponent, canActivate: [AuthGuard] },
    { path: "Settings/Customization", component: CustomizationComponent, canActivate: [AuthGuard] },
    { path: "Settings/Email", component: EmailNotificationComponent, canActivate: [AuthGuard] },
    { path: "Settings/Discord", component: DiscordComponent, canActivate: [AuthGuard] },
    { path: "Settings/Slack", component: SlackComponent, canActivate: [AuthGuard] },
    { path: "Settings/Pushover", component: PushoverComponent, canActivate: [AuthGuard] },
    { path: "Settings/Pushbullet", component: PushbulletComponent, canActivate: [AuthGuard] },
    { path: "Settings/Mattermost", component: MattermostComponent, canActivate: [AuthGuard] },
    { path: "Settings/UserManagement", component: UserManagementComponent, canActivate: [AuthGuard] },
    { path: "Settings/Update", component: UpdateComponent, canActivate: [AuthGuard] },
    { path: "Settings/CouchPotato", component: CouchPotatoComponent, canActivate: [AuthGuard] },
    { path: "Settings/DogNzb", component: DogNzbComponent, canActivate: [AuthGuard] },
    { path: "Settings/Telegram", component: TelegramComponent, canActivate: [AuthGuard] },
    { path: "Settings/Jobs", component: JobsComponent, canActivate: [AuthGuard] },
    { path: "Settings/SickRage", component: SickRageComponent, canActivate: [AuthGuard] },
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
        AuthModule,
        NgbModule,
        TooltipModule,
        NgbAccordionModule,
        AutoCompleteModule,
        CalendarModule,
        ClipboardModule,
        PipeModule,
        RadioButtonModule,
    ],
    declarations: [
        SettingsMenuComponent,
        OmbiComponent,
        PlexComponent,
        EmbyComponent,
        JobsComponent,
        LandingPageComponent,
        CustomizationComponent,
        DiscordComponent,
        SonarrComponent,
        SlackComponent,
        RadarrComponent,
        EmailNotificationComponent,
        NotificationTemplate,
        PushoverComponent,
        MattermostComponent,
        PushbulletComponent,
        UserManagementComponent,
        UpdateComponent,
        AboutComponent,
        WikiComponent,
        CouchPotatoComponent,
        DogNzbComponent,
        SickRageComponent,
        TelegramComponent,
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
    ],

})
export class SettingsModule { }
