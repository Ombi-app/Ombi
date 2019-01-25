import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { SearchService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule } from "../shared/shared.module";
import { HomeComponent } from "./home.component";
import { PopularMoviesComponent } from "./movies/popular-movies.component";
import { PopularTvComponent } from "./tv/popular-tv.component";

const routes: Routes = [
    { path: "", component: HomeComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
    ],
    declarations: [
        HomeComponent,
        PopularMoviesComponent,
        PopularTvComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        SearchService
        ],

})
export class HomeModule { }
