import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { OrderModule } from "ngx-order-pipe";

import { IdentityService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule } from "../shared/shared.module";

import { IssuesComponent } from "./issues.component";

const routes: Routes = [
    { path: "issues", component: IssuesComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        SharedModule,
        OrderModule,
    ],
    declarations: [
        IssuesComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
    ],

})
export class IssuesModule { }
