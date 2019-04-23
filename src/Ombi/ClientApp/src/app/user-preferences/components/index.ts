import { AuthGuard } from "../../auth/auth.guard";
import { Routes } from "@angular/router"
import { UserPreferenceComponent } from "./user-preference/user-preference.component";


export const components: any[] = [
    UserPreferenceComponent,
];

export const routes: Routes = [
    { path: "", component: UserPreferenceComponent, canActivate: [AuthGuard] },
];