import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";


import { MovieSearchComponent } from "./moviesearch.component";
import { MovieSearchGridComponent } from "./moviesearchgrid.component";
import { AlbumSearchComponent } from "./music/albumsearch.component";
import { ArtistSearchComponent } from "./music/artistsearch.component";
import { MusicSearchComponent } from "./music/musicsearch.component";
import { SearchComponent } from "./search.component";
import { SeriesInformationComponent } from "./seriesinformation.component";
import { TvSearchComponent } from "./tvsearch.component";

import { CardsFreeModule } from "angular-bootstrap-md";

import { RequestService } from "../services";
import { SearchService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { RemainingRequestsComponent } from "../requests/remainingrequests.component";
import { SharedModule } from "../shared/shared.module";

const routes: Routes = [
    { path: "", component: SearchComponent, canActivate: [AuthGuard] },
    { path: "show/:id", component: SeriesInformationComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        TreeTableModule,
        SharedModule,
        SidebarModule,
        TooltipModule,
        CardsFreeModule,
    ],
    declarations: [
        SearchComponent,
        MovieSearchComponent,
        TvSearchComponent,
        SeriesInformationComponent,
        MovieSearchGridComponent,
        RemainingRequestsComponent,
        MusicSearchComponent,
        ArtistSearchComponent,
        AlbumSearchComponent,
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
