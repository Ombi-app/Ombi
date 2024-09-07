import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule, Routes } from "@angular/router";

import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { SharedModule } from "../shared/shared.module";
import { UnsubscribeConfirmComponent } from "./components/confirm-component/unsubscribe-confirm.component";

const routes: Routes = [
    { path: ":id", component: UnsubscribeConfirmComponent},
];
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        SharedModule,
        RouterModule.forChild(routes),
    ],
    declarations: [
        UnsubscribeConfirmComponent,
    ],
    providers: [
    ],

})
export class UnsubscribeModule { }
