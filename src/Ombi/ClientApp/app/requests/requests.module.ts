import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { OrderModule } from "ngx-order-pipe";

import { InfiniteScrollModule } from "ngx-infinite-scroll";

import { ButtonModule, DialogModule, PaginatorModule } from "primeng/primeng";
import { MovieRequestsComponent } from "./movierequests.component";
import { MusicRequestsComponent } from "./music/musicrequests.component";
// Request
import { RequestComponent } from "./request.component";
import { TvRequestChildrenComponent } from "./tvrequest-children.component";
import { TvRequestsComponent } from "./tvrequests.component";

import { SidebarModule, TooltipModule, TreeTableModule } from "primeng/primeng";

import { IdentityService, RadarrService, RequestService, SonarrService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule } from "../shared/shared.module";

const routes: Routes = [
    { path: "", component: RequestComponent, canActivate: [AuthGuard] },
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
        PaginatorModule,
        TooltipModule,
    ],
    declarations: [
        RequestComponent,
        MovieRequestsComponent,
        TvRequestsComponent,
        TvRequestChildrenComponent,
        MusicRequestsComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
        RequestService,
        RadarrService,
        SonarrService,
        ],

})
export class RequestsModule { }
