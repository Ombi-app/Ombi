import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import {CarouselModule} from 'primeng/carousel';

import { SharedModule } from "../shared/shared.module";
import { MovieDetailsComponent } from "./components/movie/movie-details.component";
import { TvDetailsComponent } from "./components/tv/tv-details.component";
import { PipeModule } from "../pipes/pipe.module";

import * as fromComponents from './components';
import { AuthGuard } from "../auth/auth.guard";
import { ArtistDetailsComponent } from "./components/artist/artist-details.component";
import { ReactiveFormsModule } from "@angular/forms";
import { ImageComponent } from "app/components";
import { SkeletonModule } from "primeng/skeleton";


const routes: Routes = [
    { path: "movie/:movieDbId", component: MovieDetailsComponent, canActivate: [AuthGuard] },
    { path: "tv/:tvdbId/:search", component: TvDetailsComponent, canActivate: [AuthGuard] },
    { path: "tv/:tvdbId", component: TvDetailsComponent, canActivate: [AuthGuard] },
    { path: "artist/:artistId", component: ArtistDetailsComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        ReactiveFormsModule,
        PipeModule,
        CarouselModule,
        ImageComponent,
        SkeletonModule,
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
export class MediaDetailsModule { }
