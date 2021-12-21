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
import { AlbumDetailsComponent } from "./components/album/album-details.component";
import { ReactiveFormsModule } from "@angular/forms";


const routes: Routes = [
    { path: "movie/:movieDbId", component: MovieDetailsComponent, canActivate: [AuthGuard] },
    { path: "tv/:tvdbId/:search", component: TvDetailsComponent, canActivate: [AuthGuard] },
    { path: "tv/:tvdbId", component: TvDetailsComponent, canActivate: [AuthGuard] },
    { path: "artist/:artistId", component: ArtistDetailsComponent, canActivate: [AuthGuard] },
    { path: "album/:albumId", component: AlbumDetailsComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        ReactiveFormsModule,
        PipeModule,
        CarouselModule,
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
