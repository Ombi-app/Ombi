import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { OrderModule } from "ngx-order-pipe";
import { OverlayPanelModule, SharedModule, TabViewModule } from "primeng/primeng";

import { VoteService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule as OmbiShared } from "../shared/shared.module";

import { VoteComponent } from "./vote.component";

const routes: Routes = [
    { path: "", component: VoteComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        SharedModule,
        OrderModule,
        OmbiShared,
        TabViewModule,
        OverlayPanelModule,
    ],
    declarations: [
        VoteComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        VoteService,
    ],

})
export class VoteModule { }
