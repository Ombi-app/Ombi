import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { SidebarModule, TooltipModule, TreeTableModule } from "primeng/primeng";
import { RequestService } from "../services";

import { SharedModule } from "../shared/shared.module";

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        NgbModule.forRoot(),
        TreeTableModule,
        SharedModule,
        SidebarModule,
        TooltipModule,
    ],
    declarations: [
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        RequestService,
    ],
})
export class SearchModule { }
