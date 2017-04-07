import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { OmbiComponent } from './ombi/ombi.component'


const routes: Routes = [
    { path: 'Settings/Ombi', component: OmbiComponent }
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),

    ],
    declarations: [
        OmbiComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
    ]
})
export class SettingsModule { }