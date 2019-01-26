import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { SearchService } from "../services";

import { SharedModule } from "../shared/shared.module";
import { DiscoverComponent } from "./discover.component";
import { DiscoverCardComponent } from "./card/discover-card.component";

const routes: Routes = [
    { path: "", component: DiscoverComponent },
    { path: "discover", component: DiscoverComponent },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
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
