import { RouterModule, Routes } from "@angular/router";

import { AlbumSearchComponent } from "./music/albumsearch.component";
import { ArtistSearchComponent } from "./music/artistsearch.component";
import { AuthGuard } from "../auth/auth.guard";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { MovieSearchComponent } from "./moviesearch.component";
import { MovieSearchGridComponent } from "./moviesearchgrid.component";
import { MusicSearchComponent } from "./music/musicsearch.component";
import { NgModule } from "@angular/core";
import { RemainingRequestsComponent } from "../requests/remainingrequests.component";
import { RequestService } from "../services";
import { SearchComponent } from "./search.component";
import { SearchService } from "../services";
import { SeriesInformationComponent } from "./seriesinformation.component";
import { SharedModule } from "../shared/shared.module";
import { TvSearchComponent } from "./tvsearch.component";

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
