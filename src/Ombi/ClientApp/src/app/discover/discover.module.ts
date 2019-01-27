import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { SearchService } from "../services";

import { SharedModule } from "../shared/shared.module";
import { DiscoverComponent } from "./discover.component";
import { DiscoverCardComponent } from "./card/discover-card.component";
import { AuthGuard } from "../auth/auth.guard";
import { PipeModule } from "../pipes/pipe.module";

const routes: Routes = [
    { path: "", component: DiscoverComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        PipeModule,
    ],
    declarations: [
        DiscoverComponent,
        DiscoverCardComponent
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        SearchService
        ],

})
export class DiscoverModule { }
