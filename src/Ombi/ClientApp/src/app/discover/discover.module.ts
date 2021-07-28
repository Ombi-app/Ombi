import * as fromComponents from './components';

import { CarouselModule } from 'primeng/carousel';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import { NgModule } from "@angular/core";
import { PipeModule } from "../pipes/pipe.module";
import { RouterModule } from "@angular/router";
import { SharedModule } from "../shared/shared.module";
import { SkeletonModule } from 'primeng/skeleton';

@NgModule({
    imports: [
        RouterModule.forChild(fromComponents.routes),
        SharedModule,
        PipeModule,
        CarouselModule,
        MatButtonToggleModule,
        InfiniteScrollModule,
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
export class DiscoverModule { }
