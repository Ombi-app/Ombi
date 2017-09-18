import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";
import { MultiSelectModule, TooltipModule } from "primeng/primeng";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { UpdateDetailsComponent } from "./updatedetails.component";
import { UserManagementAddComponent } from "./usermanagement-add.component";
import { UserManagementEditComponent } from "./usermanagement-edit.component";
import { UserManagementComponent } from "./usermanagement.component";

import { PipeModule } from "../pipes/pipe.module";
import { IdentityService } from "../services";

import { AuthGuard } from "../auth/auth.guard";

const routes: Routes = [
    { path: "usermanagement", component: UserManagementComponent, canActivate: [AuthGuard] },
    { path: "usermanagement/add", component: UserManagementAddComponent, canActivate: [AuthGuard] },
    { path: "usermanagement/edit/:id", component: UserManagementEditComponent, canActivate: [AuthGuard] },
    { path: "usermanagement/updatedetails", component: UpdateDetailsComponent, canActivate: [AuthGuard] },
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

        TooltipModule,
    ],
    declarations: [
        UserManagementComponent,
        UserManagementAddComponent,
        UserManagementEditComponent,
        UpdateDetailsComponent,
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        IdentityService,
    ],

})
export class UserManagementModule { }
