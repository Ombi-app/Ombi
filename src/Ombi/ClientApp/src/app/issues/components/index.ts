import { AuthGuard } from "../../auth/auth.guard";
import { IssuesListComponent } from "./issues-list/issues-list.component";
import { Routes } from "@angular/router";



export const components: any[] = [
    IssuesListComponent,
];


export const entryComponents: any[] = [
];

export const providers: any[] = [
];

export const routes: Routes = [
    { path: "", component: IssuesListComponent, canActivate: [AuthGuard] },
];