import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { AuthService } from '../auth/auth.service';
import { AuthGuard } from '../auth/auth.guard';
import { AuthModule } from '../auth/auth.module';
import { SonarrService } from '../services/applications/sonarr.service';

import { OmbiComponent } from './ombi/ombi.component'
import { PlexComponent } from './plex/plex.component'
import { EmbyComponent } from './emby/emby.component'

import { SettingsMenuComponent } from './settingsmenu.component';

import { MenuModule, InputSwitchModule, InputTextModule } from 'primeng/primeng';

const routes: Routes = [
    { path: 'Settings/Ombi', component: OmbiComponent, canActivate: [AuthGuard] },
    { path: 'Settings/Plex', component: PlexComponent, canActivate: [AuthGuard] },
    { path: 'Settings/Emby', component: EmbyComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        MenuModule,
        InputSwitchModule,
        InputTextModule,
        AuthModule
    ],
    declarations: [
        SettingsMenuComponent,
        OmbiComponent,
        PlexComponent,
        EmbyComponent,
    ],
    exports: [
        RouterModule
    ],
    providers: [
        SonarrService,
        AuthService,
        AuthGuard,
    ],
   
})
export class SettingsModule { }