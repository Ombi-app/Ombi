import { Routes } from "@angular/router";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { LoginComponent } from "./login/login.component";
import { LoginOAuthComponent } from "./login/loginoauth.component";
import { CustomPageComponent } from "./custompage/custompage.component";
import { ResetPasswordComponent } from "./login/resetpassword.component";
import { TokenResetPasswordComponent } from "./login/tokenresetpassword.component";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { CookieComponent } from "./auth/cookie.component";
import { DiscoverComponent } from "./discover/components/discover/discover.component";
import { VoteComponent } from "./vote/vote.component";

export const routes: Routes = [
    { path: "*", component: PageNotFoundComponent },
    { path: "", redirectTo: "/discover", pathMatch: "full" },
    { path: "login", component: LoginComponent },
    { path: "Login/OAuth/:pin", component: LoginOAuthComponent },
    { path: "Custom", component: CustomPageComponent },
    { path: "login/:landing", component: LoginComponent },
    { path: "reset", component: ResetPasswordComponent },
    { path: "token", component: TokenResetPasswordComponent },
    { path: "landingpage", component: LandingPageComponent },
    { path: "auth/cookie", component: CookieComponent },
    { path: "discover", component: DiscoverComponent },
    { path: "vote", component: VoteComponent },
    { loadChildren: () => import("./issues/issues.module").then(m => m.IssuesModule), path: "issues" },
    { loadChildren: () => import("./settings/settings.module").then(m => m.SettingsModule), path: "Settings" },
    { loadChildren: () => import("./wizard/wizard.module").then(m => m.WizardModule), path: "Wizard" },
    { loadChildren: () => import("./usermanagement/usermanagement.module").then(m => m.UserManagementModule), path: "usermanagement" },
    // { loadChildren: () => import("./requests/requests.module").then(m => m.RequestsModule), path: "requestsOld" },
    { loadChildren: () => import("./requests-list/requests-list.module").then(m => m.RequestsListModule), path: "requests-list" },
    { loadChildren: () => import("./media-details/media-details.module").then(m => m.MediaDetailsModule), path: "details" },
    { loadChildren: () => import("./user-preferences/user-preferences.module").then(m => m.UserPreferencesModule), path: "user-preferences" },
    { loadChildren: () => import("./unsubscribe/unsubscribe.module").then(m => m.UnsubscribeModule), path: "unsubscribe" },
];
