import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { MovieSearchComponent } from "./moviesearch.component";
import { SearchComponent } from "./search.component";
import { SeriesInformationComponent } from "./seriesinformation.component";
import { TvSearchComponent } from "./tvsearch.component";

import { TreeTableModule } from "primeng/primeng";

import { RequestService } from "../services";
import { SearchService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

const routes: Routes = [
    { path: "search", component: SearchComponent, canActivate: [AuthGuard] },
    { path: "search/show/:id", component: SeriesInformationComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        TreeTableModule,
    ],
    declarations: [
        SearchComponent,
        MovieSearchComponent,
        TvSearchComponent,
        SeriesInformationComponent,
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
