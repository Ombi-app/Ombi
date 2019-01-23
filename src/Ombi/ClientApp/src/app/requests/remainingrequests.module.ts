import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

import { SidebarModule, TooltipModule, TreeTableModule } from "primeng/primeng";
import { RequestService } from "../services";

@NgModule({
    imports: [
        FormsModule,
        NgbModule.forRoot(),
        TreeTableModule,
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
