import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { SearchService } from "../services";

import { SharedModule } from "../shared/shared.module";
import { MovieDetailsComponent } from "./movie-details.component";
import { PipeModule } from "../pipes/pipe.module";

const routes: Routes = [
    { path: "movie/:movieDbId", component: MovieDetailsComponent },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        PipeModule,
    ],
    declarations: [
        MovieDetailsComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        SearchService
        ],

})
export class MediaDetailsModule { }
