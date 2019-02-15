import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { SearchService, RequestService } from "../services";

import {CarouselModule} from 'primeng/carousel';

import { SharedModule } from "../shared/shared.module";
import { MovieDetailsComponent } from "./movie-details.component";
import { PipeModule } from "../pipes/pipe.module";
import { MovieDetailsTrailerComponent } from "./movie-details-trailer.component";

const routes: Routes = [
    { path: "movie/:movieDbId", component: MovieDetailsComponent },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        PipeModule,
        CarouselModule,
    ],
    declarations: [
        MovieDetailsComponent,
        MovieDetailsTrailerComponent
    ],
    exports: [
        RouterModule,
    ],
    entryComponents: [
        MovieDetailsTrailerComponent
    ],
    providers: [
        SearchService,
        RequestService,
        ],

})
export class MediaDetailsModule { }
