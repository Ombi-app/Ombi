import { AuthGuard } from "../../auth/auth.guard";
import { Routes } from "@angular/router";
import { IssuesV2Service } from "../../services/issuesv2.service";
import { IdentityService, SearchService } from "../../services";
import { DetailsGroupComponent } from "./details-group/details-group.component";
import { IssuesDetailsComponent } from "./details/details.component";



export const components: any[] = [
    DetailsGroupComponent,
    IssuesDetailsComponent,
];

export const providers: any[] = [
    IssuesV2Service,
    IdentityService,
    SearchService,
];