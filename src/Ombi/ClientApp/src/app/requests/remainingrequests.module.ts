import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";

import { RequestService } from "../services";

@NgModule({
    imports: [
        FormsModule
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
