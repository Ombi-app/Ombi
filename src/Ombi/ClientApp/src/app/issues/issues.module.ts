import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
// import { NbChatModule, NbThemeModule } from '@nebular/theme';

import { OrderModule } from "ngx-order-pipe";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule as OmbiShared } from "../shared/shared.module";

import { IssueDetailsComponent } from "./issueDetails.component";
import { IssuesComponent } from "./issues.component";
import { IssuesTableComponent } from "./issuestable.component";
import { IssuesDetailsComponent } from "./components/details/details.component";

import { PipeModule } from "../pipes/pipe.module";

import * as fromComponents from "./components";

const routes: Routes = [
    { path: "", component: IssuesComponent, canActivate: [AuthGuard] },
    { path: ":providerId", component: IssuesDetailsComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        OrderModule,
        PipeModule,
        OmbiShared,
        // NbChatModule,
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
        ...fromComponents.providers
    ],

})
export class IssuesModule { }
