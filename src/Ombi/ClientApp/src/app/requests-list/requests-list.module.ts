import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";


import { SharedModule } from "../shared/shared.module";
import { PipeModule } from "../pipes/pipe.module";

import { AuthGuard } from "../auth/auth.guard";

import * as fromComponents from './components';
import { RequestsListComponent } from "./components/requests-list.component";

const routes: Routes = [
    { path: "", component: RequestsListComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        PipeModule,
    ],
    declarations: [
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],
    entryComponents: [
    ],
    providers: [
        ...fromComponents.providers
    ],

})
export class RequestsListModule { }
