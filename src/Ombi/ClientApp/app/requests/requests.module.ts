import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { InfiniteScrollModule } from "ngx-infinite-scroll";

import { ButtonModule, DialogModule } from "primeng/primeng";
import { MovieRequestsComponent } from "./movierequests.component";
// Request
import { RequestComponent } from "./request.component";
import { TvRequestChildrenComponent } from "./tvrequest-children.component";
import { TvRequestsComponent } from "./tvrequests.component";

import { TreeTableModule } from "primeng/primeng";

import { IdentityService } from "../services";
import { RequestService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

const routes: Routes = [
    { path: "requests", component: RequestComponent, canActivate: [AuthGuard] },
    { path: "requests/:id", component: TvRequestChildrenComponent, canActivate: [AuthGuard] },
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
        TreeTableModule,
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
    ],

})
export class RequestsModule { }
