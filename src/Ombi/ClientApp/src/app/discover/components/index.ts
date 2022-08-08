import { RadarrService, RequestService, SearchService, SonarrService } from "../../services";

import { AuthGuard } from "../../auth/auth.guard";
import { CarouselListComponent } from "./carousel-list/carousel-list.component";
import { DiscoverActorComponent } from "./actor/discover-actor.component";
import { DiscoverCardComponent } from "./card/discover-card.component";
import { DiscoverCollectionsComponent } from "./collections/discover-collections.component";
import { DiscoverComponent } from "./discover/discover.component";
import { DiscoverSearchResultsComponent } from "./search-results/search-results.component";
import { RecentlyRequestedListComponent } from "./recently-requested-list/recently-requested-list.component";
import { MatDialog } from "@angular/material/dialog";
import { RequestServiceV2 } from "../../services/requestV2.service";
import { Routes } from "@angular/router";

export const components: any[] = [
    DiscoverComponent,
    DiscoverCardComponent,
    DiscoverCollectionsComponent,
    DiscoverActorComponent,
    DiscoverSearchResultsComponent,
    CarouselListComponent,
    RecentlyRequestedListComponent,
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
    { path: "advanced/search", component: DiscoverSearchResultsComponent, canActivate: [AuthGuard] },
];