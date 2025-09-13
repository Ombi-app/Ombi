import * as fromComponents from './components';

import { CarouselModule } from 'primeng/carousel';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import { NgModule } from "@angular/core";
import { PipeModule } from "../pipes/pipe.module";
import { RouterModule } from "@angular/router";
import { SharedModule } from "../shared/shared.module";
import { SkeletonModule } from 'primeng/skeleton';
import { ImageComponent } from 'app/components';

// Import standalone components
import { DiscoverComponent } from './components/discover/discover.component';
import { DiscoverCardComponent } from './components/card/discover-card.component';
import { DiscoverCollectionsComponent } from './components/collections/discover-collections.component';
import { DiscoverActorComponent } from './components/actor/discover-actor.component';
import { DiscoverSearchResultsComponent } from './components/search-results/search-results.component';
import { CarouselListComponent } from './components/carousel-list/carousel-list.component';
import { RecentlyRequestedListComponent } from './components/recently-requested-list/recently-requested-list.component';
import { GenreButtonSelectComponent } from './components/genre/genre-button-select.component';

@NgModule({
    imports: [
        RouterModule.forChild(fromComponents.routes),
        SharedModule,
        PipeModule,
        CarouselModule,
        MatButtonToggleModule,
        InfiniteScrollModule,
        SkeletonModule,
        ImageComponent,
        // Import standalone components
        DiscoverComponent,
        DiscoverCardComponent,
        DiscoverCollectionsComponent,
        DiscoverActorComponent,
        DiscoverSearchResultsComponent,
        CarouselListComponent,
        RecentlyRequestedListComponent,
        GenreButtonSelectComponent,
    ],
    declarations: [
        // All components are now standalone - no declarations needed
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        ...fromComponents.providers
    ],

})
export class DiscoverModule { }
