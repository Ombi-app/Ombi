import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { OrderModule } from "ngx-order-pipe";
import { CalendarModule, CarouselModule, PaginatorModule, SharedModule, TabViewModule } from "primeng/primeng";

import { IdentityService, ImageService, RecentlyAddedService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { SharedModule as OmbiShared } from "../shared/shared.module";

import { RecentlyAddedComponent } from "./recentlyAdded.component";

const routes: Routes = [
    { path: "", component: RecentlyAddedComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        SharedModule,
        OrderModule,
        OmbiShared,
        PaginatorModule,
        TabViewModule,
        CalendarModule,
        CarouselModule,
    ],
    declarations: [
        RecentlyAddedComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
        RecentlyAddedService,
        ImageService,
    ],

})
export class RecentlyAddedModule { }
