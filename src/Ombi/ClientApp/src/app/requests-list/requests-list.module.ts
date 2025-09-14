import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";



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
        MatBottomSheetModule
    ],
    declarations: [
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        ...fromComponents.providers
    ],

})
export class RequestsListModule { }
