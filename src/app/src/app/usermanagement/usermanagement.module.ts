import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IdentityService, PlexService, RadarrService, SonarrService } from '../services';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from '../auth/auth.guard';
import { CommonModule } from '@angular/common';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MultiSelectModule } from 'primeng/multiselect';
import { NgModule } from '@angular/core';
import { PipeModule } from '../pipes/pipe.module';
import { SharedModule } from '../shared/shared.module';
import { SidebarModule } from 'primeng/sidebar';
import { TooltipModule } from 'primeng/tooltip';
import { UserManagementComponent } from './usermanagement.component';
import { UserManagementUserComponent } from './usermanagement-user.component';

const routes: Routes = [
	{ path: '', component: UserManagementComponent, canActivate: [AuthGuard] },
	{ path: 'user', component: UserManagementUserComponent, canActivate: [AuthGuard] },
	{ path: 'user/:id', component: UserManagementUserComponent, canActivate: [AuthGuard] },
];

@NgModule({
	imports: [
		CommonModule,
		FormsModule,
		ReactiveFormsModule,
		RouterModule.forChild(routes),
		MultiSelectModule,
		PipeModule,
		ConfirmDialogModule,
		TooltipModule,
		SidebarModule,
		SharedModule,
	],
	declarations: [UserManagementComponent, UserManagementUserComponent],
	exports: [RouterModule],
	providers: [IdentityService, PlexService, RadarrService, SonarrService],
})
export class UserManagementModule {}
