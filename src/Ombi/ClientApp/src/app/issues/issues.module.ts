import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { OrderModule } from "ngx-order-pipe";

import { IdentityService, SearchService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule as OmbiShared } from "../shared/shared.module";

import { IssueDetailsComponent } from "./issueDetails.component";
import { IssuesComponent } from "./issues.component";
import { IssuesTableComponent } from "./issuestable.component";

import { PipeModule } from "../pipes/pipe.module";

import * as fromComponents from "./components";

const routes: Routes = [
    { path: "", component: IssuesComponent, canActivate: [AuthGuard] },
    { path: ":id", component: IssueDetailsComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        OrderModule,
        PipeModule,
        OmbiShared,
    ],
    declarations: [
        IssuesComponent,
        IssueDetailsComponent,
        IssuesTableComponent,
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
        SearchService,
    ],

})
export class IssuesModule { }
