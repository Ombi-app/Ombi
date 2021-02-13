import { AuthGuard } from "../../auth/auth.guard";
import { IssuesListComponent } from "./issues-list/issues-list.component";
import { Routes } from "@angular/router";
import { IssuesV2Service } from "../../services/issuesv2.service";
import { IdentityService, SearchService } from "../../services";
import { DetailsGroupComponent } from "./details-group/details-group.component";



export const components: any[] = [
    IssuesListComponent,
    DetailsGroupComponent,
];

export const providers: any[] = [
    IssuesV2Service,
    IdentityService,
    SearchService,
];

export const routes: Routes = [
    { path: "", component: IssuesListComponent, canActivate: [AuthGuard] },
];