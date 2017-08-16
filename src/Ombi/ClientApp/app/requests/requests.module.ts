import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { InfiniteScrollModule } from 'ngx-infinite-scroll';

import { ButtonModule, DialogModule } from 'primeng/primeng';
// Request
import { RequestComponent } from './request.component';
import { MovieRequestsComponent } from './movierequests.component';
import { TvRequestsComponent } from './tvrequests.component';
import { TvRequestManageComponent } from './tvrequest-manage.component';
//import { RequestGridComponent } from '../request-grid/request-grid.component';
// import { RequestCardComponent } from '../request-grid/request-card.component';
import { TreeTableModule } from 'primeng/primeng';

import { IdentityService } from '../services/identity.service';
import { RequestService } from '../services/request.service';

import { AuthGuard } from '../auth/auth.guard';

const routes: Routes = [
    { path: 'requests', component: RequestComponent, canActivate: [AuthGuard] },
    { path: 'requests/:id', component: TvRequestManageComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        InfiniteScrollModule,
        ButtonModule,
        DialogModule,
        TreeTableModule
    ],
    declarations: [
        RequestComponent,
        MovieRequestsComponent,
        TvRequestsComponent,
        TvRequestManageComponent,
    ],
    exports: [
        RouterModule
    ],
    providers: [
        IdentityService,
        RequestService
    ],

})
export class RequestsModule { }