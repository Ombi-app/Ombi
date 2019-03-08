import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { SearchService, RequestService, RadarrService } from "../services";

import {CarouselModule} from 'primeng/carousel';

import { SharedModule } from "../shared/shared.module";
import { MovieDetailsComponent } from "./components/movie/movie-details.component";
import { TvDetailsComponent } from "./components/tv/tv-details.component";
import { PipeModule } from "../pipes/pipe.module";
import { YoutubeTrailerComponent } from "./components/shared/youtube-trailer.component";

import * as fromComponents from './components';
import { AuthGuard } from "../auth/auth.guard";


const routes: Routes = [
    { path: "movie/:movieDbId", component: MovieDetailsComponent, canActivate: [AuthGuard] },
    { path: "tv/:tvdbId/:search", component: TvDetailsComponent, canActivate: [AuthGuard] },
    { path: "tv/:tvdbId", component: TvDetailsComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        PipeModule,
        CarouselModule,
    ],
    declarations: [
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],
    entryComponents: [
        YoutubeTrailerComponent
    ],
    providers: [
        SearchService,
        RequestService,
        RadarrService,
        ],

})
export class MediaDetailsModule { }
