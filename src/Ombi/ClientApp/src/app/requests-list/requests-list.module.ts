import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";


import { SharedModule } from "../shared/shared.module";
import { PipeModule } from "../pipes/pipe.module";

import { AuthGuard } from "../auth/auth.guard";

import * as fromComponents from './components';
import { RequestsListComponent } from "./components/requests-list.component";
import { MatBottomSheetModule } from "@angular/material/bottom-sheet";

const routes: Routes = [
    { path: "", component: RequestsListComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        PipeModule,
        MatBottomSheetModule
    ],
    declarations: [
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],
    entryComponents: [
        ...fromComponents.entryComponents
    ],
    providers: [
        ...fromComponents.providers
    ],

})
export class RequestsListModule { }
