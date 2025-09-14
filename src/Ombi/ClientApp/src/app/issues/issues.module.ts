import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
// import { NbChatModule, NbThemeModule } from '@nebular/theme';

import { AuthGuard } from '../auth/auth.guard';

import { IssueDetailsComponent } from './issueDetails.component';
import { IssuesComponent } from './issues.component';
import { IssuesTableComponent } from './issuestable.component';
import { IssuesDetailsComponent } from './components/details/details.component';

import * as fromComponents from './components';

const routes: Routes = [
	{ path: '', component: IssuesComponent, canActivate: [AuthGuard] },
	{ path: ':providerId', component: IssuesDetailsComponent, canActivate: [AuthGuard] },
];

@NgModule({
	imports: [
		RouterModule.forChild(routes),
		// NbChatModule,
	],
	declarations: [IssuesComponent, IssueDetailsComponent, IssuesTableComponent, ...fromComponents.components],
	exports: [RouterModule],
	providers: [...fromComponents.providers],
})
export class IssuesModule {}
