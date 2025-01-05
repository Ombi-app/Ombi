import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";

import { MatStepperModule } from "@angular/material/stepper";

import { CreateAdminComponent } from "./createadmin/createadmin.component";
import { EmbyComponent } from "./emby/emby.component";
import { JellyfinComponent } from "./jellyfin/jellyfin.component";
import { MediaServerComponent } from "./mediaserver/mediaserver.component";
import { PlexComponent } from "./plex/plex.component";
import { WelcomeComponent } from "./welcome/welcome.component";
import { OmbiConfigComponent } from "./ombiconfig/ombiconfig.component";
import { DatabaseComponent } from "./database/database.component";

import { EmbyService } from "../services";
import { JellyfinService } from "../services";
import { PlexService } from "../services";
import { IdentityService } from "../services";
import { PlexOAuthService } from "../services";
import { WizardService } from "./services/wizard.service";

import { SharedModule } from "../shared/shared.module";

const routes: Routes = [
    { path: "", component: WelcomeComponent},
    { path: "MediaServer", component: MediaServerComponent},
    { path: "Plex", component: PlexComponent},
    { path: "Emby", component: EmbyComponent},
    { path: "Jellyfin", component: JellyfinComponent},
    { path: "CreateAdmin", component: CreateAdminComponent},
    { path: "OmbiConfig", component: OmbiConfigComponent},
];
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        SharedModule,
        MatStepperModule,
        RouterModule.forChild(routes),
    ],
    declarations: [
        WelcomeComponent,
        MediaServerComponent,
        PlexComponent,
        CreateAdminComponent,
        EmbyComponent,
        JellyfinComponent,
        OmbiConfigComponent,
        DatabaseComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        PlexService,
        IdentityService,
        EmbyService,
        JellyfinService,
        PlexOAuthService,
        WizardService,
    ],

})
export class WizardModule { }
