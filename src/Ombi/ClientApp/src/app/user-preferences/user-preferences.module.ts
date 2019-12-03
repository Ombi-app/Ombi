import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router"
import { QRCodeModule } from 'angularx-qrcode';

import { SharedModule } from "../shared/shared.module";

import * as fromComponents from './components';


@NgModule({
    imports: [
        RouterModule.forChild(fromComponents.routes),
        SharedModule,
        QRCodeModule,
    ],
    declarations: [
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        ],

})
export class UserPreferencesModule { }
