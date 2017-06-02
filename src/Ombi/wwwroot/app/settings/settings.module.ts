import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { AuthService } from '../auth/auth.service';
import { AuthGuard } from '../auth/auth.guard';
import { AuthModule } from '../auth/auth.module';
import { SonarrService } from '../services/applications/sonarr.service';
import { RadarrService } from '../services/applications/radarr.service';

import { OmbiComponent } from './ombi/ombi.component';
import { PlexComponent } from './plex/plex.component';
import { EmbyComponent } from './emby/emby.component';
import { SonarrComponent } from './sonarr/sonarr.component';
import { RadarrComponent } from './radarr/radarr.component';
import { LandingPageComponent } from './landingpage/landingpage.component';
import { CustomizationComponent } from './customization/customization.component';

import { SettingsMenuComponent } from './settingsmenu.component';

import { MenuModule, InputSwitchModule, InputTextModule } from 'primeng/primeng';

const routes: Routes = [
    { path: 'Settings/Ombi', component: OmbiComponent, canActivate: [AuthGuard] },
    { path: 'Settings/Plex', component: PlexComponent, canActivate: [AuthGuard] },
    { path: 'Settings/Emby', component: EmbyComponent, canActivate: [AuthGuard] },
    { path: 'Settings/Sonarr', component: SonarrComponent, canActivate: [AuthGuard] },
    { path: 'Settings/Radarr', component: RadarrComponent, canActivate: [AuthGuard] },
    { path: 'Settings/LandingPage', component: LandingPageComponent, canActivate: [AuthGuard] },
    { path: 'Settings/Customization', component: CustomizationComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        MenuModule,
        InputSwitchModule,
        InputTextModule,
        AuthModule,
        NgbModule
    ],
    declarations: [
        SettingsMenuComponent,
        OmbiComponent,
        PlexComponent,
        EmbyComponent,
        LandingPageComponent,
        CustomizationComponent,
        SonarrComponent,
        RadarrComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
        SonarrService,
        AuthService,
        RadarrService,
        AuthGuard,
    ],
   
})
export class SettingsModule { }