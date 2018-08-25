import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { MovieSearchComponent } from "./moviesearch.component";
import { MovieSearchGridComponent } from "./moviesearchgrid.component";
import { SearchComponent } from "./search.component";
import { SeriesInformationComponent } from "./seriesinformation.component";
import { TvSearchComponent } from "./tvsearch.component";

import { SidebarModule, TooltipModule, TreeTableModule } from "primeng/primeng";

import { RequestService } from "../services";
import { SearchService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule } from "../shared/shared.module";
import { RemainingRequestsComponent } from "../requests/remainingrequests.component";

const routes: Routes = [
    { path: "", component: SearchComponent, canActivate: [AuthGuard] },
    { path: "show/:id", component: SeriesInformationComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        TreeTableModule,
        SharedModule,
        SidebarModule,
        TooltipModule,
    ],
    declarations: [
        SearchComponent,
        MovieSearchComponent,
        TvSearchComponent,
        SeriesInformationComponent,
        MovieSearchGridComponent,
        RemainingRequestsComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        SearchService,
        RequestService,
    ],
})
export class SearchModule { }
