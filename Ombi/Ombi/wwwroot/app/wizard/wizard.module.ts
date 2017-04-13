import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { WelcomeComponent } from './welcome/welcome.component';
import { MediaServerComponent } from './mediaserver/mediaserver.component';

const routes: Routes = [
    { path: 'Wizard', component: WelcomeComponent},
    { path: 'Wizard/MediaServer', component: MediaServerComponent},
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        WelcomeComponent,
        MediaServerComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
    ],
   
})
export class WizardModule { }