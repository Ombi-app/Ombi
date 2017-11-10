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

const routes: Routes = [
    { path: "Wizard", component: WelcomeComponent},
    { path: "Wizard/MediaServer", component: MediaServerComponent},
    { path: "Wizard/Plex", component: PlexComponent},
    { path: "Wizard/Emby", component: EmbyComponent},
    { path: "Wizard/CreateAdmin", component: CreateAdminComponent},
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
    ],

})
export class WizardModule { }
