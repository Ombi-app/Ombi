import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";

import {ConfirmationService, ConfirmDialogModule } from "primeng/primeng";

import { CreateAdminComponent } from "./createadmin/createadmin.component";
import { EmbyComponent } from "./emby/emby.component";
import { MediaServerComponent } from "./mediaserver/mediaserver.component";
import { PlexComponent } from "./plex/plex.component";
import { WelcomeComponent } from "./welcome/welcome.component";

import { EmbyService } from "../services";
import { PlexService } from "../services";
import { IdentityService } from "../services";
import { PlexOAuthService } from "../services";

const routes: Routes = [
    { path: "", component: WelcomeComponent},
    { path: "MediaServer", component: MediaServerComponent},
    { path: "Plex", component: PlexComponent},
    { path: "Emby", component: EmbyComponent},
    { path: "CreateAdmin", component: CreateAdminComponent},
];
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ConfirmDialogModule,
        RouterModule.forChild(routes),
    ],
    declarations: [
        WelcomeComponent,
        MediaServerComponent,
        PlexComponent,
        CreateAdminComponent,
        EmbyComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        PlexService,
        IdentityService,
        EmbyService,
        ConfirmationService,
        PlexOAuthService,
    ],

})
export class WizardModule { }
