import { DiscoverComponent } from "./discover/discover.component";
import { DiscoverCollectionsComponent } from "./collections/discover-collections.component";
import { DiscoverActorComponent } from "./actor/discover-actor.component";
import { DiscoverCardComponent } from "./card/discover-card.component";
import { Routes } from "@angular/router";
import { AuthGuard } from "../../auth/auth.guard";
import { SearchService, RequestService, SonarrService, RadarrService } from "../../services";
import { MatDialog } from "@angular/material/dialog";
import { DiscoverSearchResultsComponent } from "./search-results/search-results.component";
import { CarouselListComponent } from "./carousel-list/carousel-list.component";
import { RequestServiceV2 } from "../../services/requestV2.service";


export const components: any[] = [
    DiscoverComponent,
    DiscoverCardComponent,
    DiscoverCollectionsComponent,
    DiscoverActorComponent,
    DiscoverSearchResultsComponent,
    CarouselListComponent,
];

export const providers: any[] = [
    SearchService,
    MatDialog,
    RequestService,
    RequestServiceV2,
    SonarrService,
    RadarrService,
];

export const routes: Routes = [
    { path: "", component: DiscoverComponent, canActivate: [AuthGuard] },
    { path: "collection/:collectionId", component: DiscoverCollectionsComponent, canActivate: [AuthGuard] },
    { path: "actor/:actorId", component: DiscoverActorComponent, canActivate: [AuthGuard] },
    { path: ":searchTerm", component: DiscoverSearchResultsComponent, canActivate: [AuthGuard] },
];