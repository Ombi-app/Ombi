import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { UserManagementComponent } from './usermanagement.component';
import { UserManagementEditComponent } from './usermanagement-edit.component';
import { UserManagementAddComponent } from './usermanagement-add.component';

import { IdentityService } from '../services/identity.service';

import { AuthGuard } from '../auth/auth.guard';

const routes: Routes = [
    { path: 'usermanagement', component: UserManagementComponent, canActivate: [AuthGuard] },
    { path: 'usermanagement/add', component: UserManagementAddComponent, canActivate: [AuthGuard] },
    { path: 'usermanagement/edit/:id', component: UserManagementEditComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
    ],
    declarations: [
        UserManagementComponent,
        UserManagementAddComponent,
        UserManagementEditComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
        IdentityService
    ],
   
})
export class UserManagementModule { }