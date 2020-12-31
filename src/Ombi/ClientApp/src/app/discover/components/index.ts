import { DiscoverComponent } from "./discover/discover.component";
import { DiscoverCardDetailsComponent } from "./card/discover-card-details.component";
import { DiscoverCollectionsComponent } from "./collections/discover-collections.component";
import { DiscoverActorComponent } from "./actor/discover-actor.component";
import { DiscoverCardComponent } from "./card/discover-card.component";
import { Routes } from "@angular/router";
import { AuthGuard } from "../../auth/auth.guard";
import { SearchService, RequestService } from "../../services";
import { MatDialog } from "@angular/material/dialog";
import { DiscoverGridComponent } from "./grid/discover-grid.component";
import { MovieListComponent } from "./movie-list/movie-list.component";


export const components: any[] = [
    DiscoverComponent,
    DiscoverCardComponent,
    DiscoverCardDetailsComponent,
    DiscoverCollectionsComponent,
    DiscoverActorComponent,
    DiscoverGridComponent,
    MovieListComponent,
];


export const entryComponents: any[] = [
    DiscoverCardDetailsComponent
];

export const providers: any[] = [
    SearchService,
    MatDialog,
    RequestService,
];

export const routes: Routes = [
    { path: "", component: DiscoverComponent, canActivate: [AuthGuard] },
    { path: "collection/:collectionId", component: DiscoverCollectionsComponent, canActivate: [AuthGuard] },
    { path: "actor/:actorId", component: DiscoverActorComponent, canActivate: [AuthGuard] }
];