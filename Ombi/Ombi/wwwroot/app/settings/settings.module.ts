import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { OmbiComponent } from './ombi/ombi.component'

import { SettingsMenuComponent } from './settingsmenu.component';

import { MenuModule, InputSwitchModule, InputTextModule } from 'primeng/primeng';

const routes: Routes = [
    { path: 'Settings/Ombi', component: OmbiComponent }
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        MenuModule,
        InputSwitchModule,
        InputTextModule,

    ],
    declarations: [
        SettingsMenuComponent,
        OmbiComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
    ],
   
})
export class SettingsModule { }