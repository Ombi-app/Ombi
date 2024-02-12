import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { OverlayPanelModule } from 'primeng/overlaypanel';
import { TabViewModule } from 'primeng/tabview';

import { VoteService } from '../services';

import { AuthGuard } from '../auth/auth.guard';

import { SharedModule as OmbiShared } from '../shared/shared.module';

import { VoteComponent } from './vote.component';

const routes: Routes = [{ path: '', component: VoteComponent, canActivate: [AuthGuard] }];

@NgModule({
	imports: [RouterModule.forChild(routes), OmbiShared, TabViewModule, OverlayPanelModule],
	declarations: [VoteComponent],
	exports: [RouterModule],
	providers: [VoteService],
})
export class VoteModule {}
