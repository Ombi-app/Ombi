import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { UserManagementComponent } from './usermanagement.component';
import { UserManagementEditComponent } from './usermanagement-edit.component';
import { UserManagementAddComponent } from './usermanagement-add.component';
import { UpdateDetailsComponent } from './updatedetails.component';

import { IdentityService } from '../services/identity.service';

import { AuthGuard } from '../auth/auth.guard';

const routes: Routes = [
    { path: 'usermanagement', component: UserManagementComponent, canActivate: [AuthGuard] },
    { path: 'usermanagement/add', component: UserManagementAddComponent, canActivate: [AuthGuard] },
    { path: 'usermanagement/edit/:id', component: UserManagementEditComponent, canActivate: [AuthGuard] },
    { path: 'usermanagement/updatedetails', component: UpdateDetailsComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
    ],
    declarations: [
        UserManagementComponent,
        UserManagementAddComponent,
        UserManagementEditComponent,
        UpdateDetailsComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
        IdentityService
    ],
   
})
export class UserManagementModule { }