import * as fromComponents from './';

import { CarouselModule } from 'primeng/carousel';
import { CommonModule } from '@angular/common';
import { InfiniteScrollModule } from 'ngx-infinite-scroll';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import { NgModule } from "@angular/core";
import { PipeModule } from "../pipes/pipe.module";
import { RouterModule } from "@angular/router";
import { SharedModule } from "../shared/shared.module";
import { SkeletonModule } from 'primeng/skeleton';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
    imports: [
        SharedModule,
        SkeletonModule,
        TranslateModule,
        RouterModule,
        CommonModule,
    ],
    declarations: [
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],

})
export class ComponentsModule { }
