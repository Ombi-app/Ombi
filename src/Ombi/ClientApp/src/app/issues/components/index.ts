import { AuthGuard } from "../../auth/auth.guard";
import { IssuesListComponent } from "./issues-list/issues-list.component";
import { Routes } from "@angular/router";
import { IssuesV2Service } from "../../services/issuesv2.service";
import { IdentityService, SearchService } from "../../services";



export const components: any[] = [
    IssuesListComponent,
];


export const entryComponents: any[] = [
];

export const providers: any[] = [
    IssuesV2Service,
    IdentityService,
    SearchService,
];

export const routes: Routes = [
    { path: "", component: IssuesListComponent, canActivate: [AuthGuard] },
];