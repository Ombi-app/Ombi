import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { OrderModule } from "ngx-order-pipe";

import { InfiniteScrollModule } from "ngx-infinite-scroll";

import { ButtonModule, DialogModule } from "primeng/primeng";
import { MovieRequestsComponent } from "./movierequests.component";
// Request
import { RequestComponent } from "./request.component";
import { TvRequestChildrenComponent } from "./tvrequest-children.component";
import { TvRequestsComponent } from "./tvrequests.component";

import { SidebarModule, TreeTableModule } from "primeng/primeng";

import { IdentityService, RadarrService, RequestService, SonarrService, SearchService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule } from "../shared/shared.module";

const routes: Routes = [
    { path: "", component: RequestComponent, canActivate: [AuthGuard] },
    { path: ":id", component: TvRequestChildrenComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        InfiniteScrollModule,
        ButtonModule,
        DialogModule,
        TreeTableModule,
        SharedModule,
        SidebarModule,
        OrderModule,
    ],
    declarations: [
        RequestComponent,
        MovieRequestsComponent,
        TvRequestsComponent,
        TvRequestChildrenComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
        RequestService,
        RadarrService,
        SonarrService,
        SearchService,
        ],

})
export class RequestsModule { }
