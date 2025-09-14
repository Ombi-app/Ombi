import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router"

import { MatCheckboxModule } from '@angular/material/checkbox';

import { QRCodeModule } from 'angularx-qrcode';

import * as fromComponents from './components';
import { ReactiveFormsModule } from "@angular/forms";
import { ValidationService } from "../services";


@NgModule({
    imports: [
        RouterModule.forChild(fromComponents.routes),
        ReactiveFormsModule,
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
        ValidationService,
    ],

})
export class UserPreferencesModule { }
