import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { OrderModule } from "ngx-order-pipe";

import { IdentityService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule } from "../shared/shared.module";

import { IssueDetailsComponent } from "./issueDetails.component";
import { IssuesComponent } from "./issues.component";

import { PipeModule } from "../pipes/pipe.module";

const routes: Routes = [
    { path: "", component: IssuesComponent, canActivate: [AuthGuard] },
    { path: ":id", component: IssueDetailsComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        SharedModule,
        OrderModule,
        PipeModule,
    ],
    declarations: [
        IssuesComponent,
        IssueDetailsComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
    ],

})
export class IssuesModule { }
