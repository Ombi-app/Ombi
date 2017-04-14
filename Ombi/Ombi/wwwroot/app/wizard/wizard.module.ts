import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { WelcomeComponent } from './welcome/welcome.component';
import { MediaServerComponent } from './mediaserver/mediaserver.component';
import { PlexComponent } from './plex/plex.component';

import { PlexService } from '../services/plex.service';

const routes: Routes = [
    { path: 'Wizard', component: WelcomeComponent},
    { path: 'Wizard/MediaServer', component: MediaServerComponent},
    { path: 'Wizard/Plex', component: PlexComponent},
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
        PlexComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
        PlexService
    ],
   
})
export class WizardModule { }