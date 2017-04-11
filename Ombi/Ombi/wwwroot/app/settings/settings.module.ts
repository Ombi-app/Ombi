import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { AuthService } from '../auth/auth.service';
import { AuthGuard } from '../auth/auth.guard';
import { AuthModule } from '../auth/auth.module';

import { OmbiComponent } from './ombi/ombi.component'

import { SettingsMenuComponent } from './settingsmenu.component';

import { MenuModule, InputSwitchModule, InputTextModule } from 'primeng/primeng';

const routes: Routes = [
    { path: 'Settings/Ombi', component: OmbiComponent, canActivate: [AuthGuard] }
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
        OmbiComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [

        AuthService,
        AuthGuard,
    ],
   
})
export class SettingsModule { }