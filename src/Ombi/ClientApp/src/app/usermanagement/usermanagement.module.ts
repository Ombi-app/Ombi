﻿import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { ConfirmationService, ConfirmDialogModule, MultiSelectModule, SidebarModule, TooltipModule } from "primeng/primeng";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { UpdateDetailsComponent } from "./updatedetails.component";
import { UserManagementUserComponent } from "./usermanagement-user.component";
import { UserManagementComponent } from "./usermanagement.component";

import { PipeModule } from "../pipes/pipe.module";
import { IdentityService, PlexService, RadarrService, SonarrService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { OrderModule } from "ngx-order-pipe";

import { SharedModule } from "../shared/shared.module";

const routes: Routes = [
    { path: "", component: UserManagementComponent, canActivate: [AuthGuard] },
    { path: "user", component: UserManagementUserComponent, canActivate: [AuthGuard] },
    { path: "user/:id", component: UserManagementUserComponent, canActivate: [AuthGuard] },
    { path: "updatedetails", component: UpdateDetailsComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
        MultiSelectModule,
        PipeModule,
        ConfirmDialogModule,
        TooltipModule,
        OrderModule,
        SidebarModule,
        SharedModule,
    ],
    declarations: [
        UserManagementComponent,
        UpdateDetailsComponent,
        UserManagementUserComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
        ConfirmationService,
        PlexService,
        RadarrService,
        SonarrService,
    ],

})
export class UserManagementModule { }
