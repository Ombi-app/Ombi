// import { NgModule } from "@angular/core";
// import { RouterModule, Routes } from "@angular/router";
// import { OrderModule } from "ngx-order-pipe";

// import { InfiniteScrollModule } from "ngx-infinite-scroll";

// import { MovieRequestsComponent } from "./movierequests.component";
// import { MusicRequestsComponent } from "./music/musicrequests.component";
// // Request
// import { RequestComponent } from "./request.component";
// import { TvRequestChildrenComponent } from "./tvrequest-children.component";
// import { TvRequestsComponent } from "./tvrequests.component";


// import { IdentityService, RadarrService, RequestService, SonarrService } from "../services";

// import { AuthGuard } from "../auth/auth.guard";

// import { SharedModule } from "../shared/shared.module";

// const routes: Routes = [
//     { path: "", component: RequestComponent, canActivate: [AuthGuard] },
// ];
// @NgModule({
//     imports: [
//         RouterModule.forChild(routes),
//         InfiniteScrollModule,
//         SharedModule,
//         OrderModule,
//     ],
//     declarations: [
//         RequestComponent,
//         MovieRequestsComponent,
//         TvRequestsComponent,
//         TvRequestChildrenComponent,
//         MusicRequestsComponent,
//     ],
//     exports: [
//         RouterModule,
//     ],
//     providers: [
//         IdentityService,
//         RequestService,
//         RadarrService,
//         SonarrService,
//         ],

// })
// export class RequestsModule { }
