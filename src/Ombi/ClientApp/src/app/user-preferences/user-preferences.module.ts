import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router"

import { MatCheckboxModule } from '@angular/material/checkbox';

import { SharedModule } from "../shared/shared.module";
import { QRCodeModule } from 'angularx-qrcode';

import * as fromComponents from './components';


@NgModule({
    imports: [
        RouterModule.forChild(fromComponents.routes),
        SharedModule,
        MatCheckboxModule,
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
