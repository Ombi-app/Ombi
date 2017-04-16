import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { WelcomeComponent } from './welcome/welcome.component';
import { MediaServerComponent } from './mediaserver/mediaserver.component';
import { PlexComponent } from './plex/plex.component';
import { CreateAdminComponent } from './createadmin/createadmin.component';
import { EmbyComponent } from './emby/emby.component';

import { PlexService } from '../services/applications/plex.service';
import { EmbyService } from '../services/applications/emby.service';
import { IdentityService } from '../services/identity.service';

const routes: Routes = [
    { path: 'Wizard', component: WelcomeComponent},
    { path: 'Wizard/MediaServer', component: MediaServerComponent},
    { path: 'Wizard/Plex', component: PlexComponent},
    { path: 'Wizard/Emby', component: EmbyComponent},
    { path: 'Wizard/CreateAdmin', component: CreateAdminComponent},
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        WelcomeComponent,
        MediaServerComponent,
        PlexComponent,
        CreateAdminComponent,
        EmbyComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
        PlexService,
        IdentityService,
        EmbyService
    ],
   
})
export class WizardModule { }