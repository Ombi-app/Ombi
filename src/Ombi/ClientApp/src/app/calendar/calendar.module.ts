import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { RequestService } from "../services";

import { SharedModule } from "../shared/shared.module";
import { PipeModule } from "../pipes/pipe.module";

import * as fromComponents from './components';
import { AuthGuard } from "../auth/auth.guard";
import { CalendarComponent } from "./components/calendar.component";

import { FullCalendarModule } from 'primeng/fullcalendar';
import { CalendarService } from "../services/calendar.service";


const routes: Routes = [
    { path: "", component: CalendarComponent, canActivate: [AuthGuard] },
];
@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SharedModule,
        PipeModule,
        FullCalendarModule,
    ],
    declarations: [
        ...fromComponents.components
    ],
    exports: [
        RouterModule,
    ],
    providers: [
        RequestService,
        CalendarService
    ],

})
export class CalendarModule { }
