import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";

import { SidebarModule, TooltipModule, TreeTableModule } from "primeng/primeng";
import { RequestService } from "../services";

@NgModule({
    imports: [
        FormsModule,
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
