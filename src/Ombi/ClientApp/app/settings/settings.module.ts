import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { NgbAccordionModule, NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { ClipboardModule } from "ngx-clipboard/dist";

import { AuthGuard } from "../auth/auth.guard";
import { AuthModule } from "../auth/auth.module";
import { AuthService } from "../auth/auth.service";
import { RadarrService } from "../services";
import { SonarrService } from "../services";
import { TesterService } from "../services";
import { ValidationService } from "../services";

import { PipeModule } from "../pipes/pipe.module";
import { CustomizationComponent } from "./customization/customization.component";
import { EmbyComponent } from "./emby/emby.component";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { DiscordComponent } from "./notifications/discord.component";
import { EmailNotificationComponent } from "./notifications/emailnotification.component";
import { MattermostComponent } from "./notifications/mattermost.component";
import { NotificationTemplate } from "./notifications/notificationtemplate.component";
import { PushbulletComponent } from "./notifications/pushbullet.component";
import { PushoverComponent } from "./notifications/pushover.component";
import { SlackComponent } from "./notifications/slack.component";
import { OmbiComponent } from "./ombi/ombi.component";
import { PlexComponent } from "./plex/plex.component";
import { RadarrComponent } from "./radarr/radarr.component";
import { SonarrComponent } from "./sonarr/sonarr.component";
import { UserManagementComponent } from "./usermanagement/usermanagement.component";

import { SettingsMenuComponent } from "./settingsmenu.component";

import { AutoCompleteModule, CalendarModule, InputSwitchModule, InputTextModule, MenuModule, TooltipModule } from "primeng/primeng";

const routes: Routes = [
    { path: "Settings/Ombi", component: OmbiComponent, canActivate: [AuthGuard] },
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
    ],
    declarations: [
        SettingsMenuComponent,
        OmbiComponent,
        PlexComponent,
        EmbyComponent,
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
    ],

})
export class SettingsModule { }
