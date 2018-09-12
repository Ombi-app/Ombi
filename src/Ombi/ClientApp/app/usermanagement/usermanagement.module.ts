import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { ConfirmationService, ConfirmDialogModule, MultiSelectModule, SidebarModule, TooltipModule } from "primeng/primeng";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { UpdateDetailsComponent } from "./updatedetails.component";
import { UserManagementAddComponent } from "./usermanagement-add.component";
import { UserManagementEditComponent } from "./usermanagement-edit.component";
import { UserManagementComponent } from "./usermanagement.component";

import { PipeModule } from "../pipes/pipe.module";
import { IdentityService, PlexService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

import { OrderModule } from "ngx-order-pipe";
import { AddPlexUserComponent } from "./addplexuser.component";

import { SharedModule } from "../shared/shared.module";

const routes: Routes = [
    { path: "", component: UserManagementComponent, canActivate: [AuthGuard] },
    { path: "add", component: UserManagementAddComponent, canActivate: [AuthGuard] },
    { path: "edit/:id", component: UserManagementEditComponent, canActivate: [AuthGuard] },
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
        UserManagementAddComponent,
        UserManagementEditComponent,
        UpdateDetailsComponent,
        AddPlexUserComponent,
    ],
    entryComponents:[
        AddPlexUserComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
        ConfirmationService,
        PlexService,
    ],

})
export class UserManagementModule { }
