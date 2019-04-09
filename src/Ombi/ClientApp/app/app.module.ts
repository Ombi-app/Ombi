import { CommonModule, PlatformLocation } from "@angular/common";
import { HttpClient, HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpModule } from "@angular/http";
import { MatButtonModule, MatCardModule, MatInputModule, MatTabsModule } from "@angular/material";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";

import { JwtModule } from "@auth0/angular-jwt";

import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { CookieService } from "ng2-cookies";
import { NgxEditorModule } from "ngx-editor";
import { GrowlModule } from "primeng/components/growl/growl";
import { ButtonModule, CaptchaModule, ConfirmationService, ConfirmDialogModule, DataTableModule, DialogModule, OverlayPanelModule, SharedModule, SidebarModule,
    TooltipModule } from "primeng/primeng";

// Components
import { AppComponent } from "./app.component";

import { CookieComponent } from "./auth/cookie.component";
import { CustomPageComponent } from "./custompage/custompage.component";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { LoginComponent } from "./login/login.component";
import { LoginOAuthComponent } from "./login/loginoauth.component";
import { ResetPasswordComponent } from "./login/resetpassword.component";
import { TokenResetPasswordComponent } from "./login/tokenresetpassword.component";

// Services
import { AuthGuard } from "./auth/auth.guard";
import { AuthService } from "./auth/auth.service";
import { IdentityService } from "./services";
import { ImageService } from "./services";
import { LandingPageService } from "./services";
import { NotificationService } from "./services";
import { SettingsService } from "./services";
import { CustomPageService, IssuesService, JobService, PlexTvService, StatusService } from "./services";

const routes: Routes = [
    { path: "*", component: PageNotFoundComponent },
    { path: "", redirectTo: "/search", pathMatch: "full" },
    { path: "login", component: LoginComponent },
    { path: "Login/OAuth/:pin", component: LoginOAuthComponent },
    { path: "Custom", component: CustomPageComponent },
    { path: "login/:landing", component: LoginComponent },
    { path: "reset", component: ResetPasswordComponent },
    { path: "token", component: TokenResetPasswordComponent },
    { path: "landingpage", component: LandingPageComponent },
    { path: "auth/cookie", component: CookieComponent },
    { loadChildren: "./issues/issues.module#IssuesModule", path: "issues" },
    { loadChildren: "./settings/settings.module#SettingsModule", path: "Settings" },
    { loadChildren: "./wizard/wizard.module#WizardModule", path: "Wizard" },
    { loadChildren: "./usermanagement/usermanagement.module#UserManagementModule", path: "usermanagement" },
    { loadChildren: "./requests/requests.module#RequestsModule", path: "requests" },
    { loadChildren: "./search/search.module#SearchModule", path: "search" },
    { loadChildren: "./recentlyAdded/recentlyAdded.module#RecentlyAddedModule", path: "recentlyadded" },
    { loadChildren: "./vote/vote.module#VoteModule", path: "vote" },
];

// AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient, platformLocation: PlatformLocation) {
    const base = platformLocation.getBaseHrefFromDOM();
    const version = Math.floor(Math.random() * 999999999);
    if (base.length > 1) {
        return new TranslateHttpLoader(http, `${base}/translations/`, `.json?v=${version}`);
    }
    return new TranslateHttpLoader(http, "/translations/", `.json?v=${version}`);
}

export function JwtTokenGetter() {
    const token = localStorage.getItem("id_token");
    if (!token) {
        return "";
    }
    return token;
}

@NgModule({
    imports: [
        RouterModule.forRoot(routes),
        BrowserModule,
        HttpClientModule,
        BrowserAnimationsModule,
        HttpModule,
        GrowlModule,
        ButtonModule,
        FormsModule,
        DataTableModule,
        SharedModule,
        NgxEditorModule,
        DialogModule,
        MatButtonModule,
        NgbModule.forRoot(),
        MatCardModule,
        MatInputModule,
        MatTabsModule,
        ReactiveFormsModule,
        CaptchaModule,
        TooltipModule,
        ConfirmDialogModule,
        OverlayPanelModule,
        CommonModule,
        JwtModule.forRoot({
            config: {
                tokenGetter: JwtTokenGetter,
            },
        }),
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient, PlatformLocation],
            },
        }),
        SidebarModule,
    ],
    declarations: [
        AppComponent,
        PageNotFoundComponent,
        LoginComponent,
        LandingPageComponent,
        ResetPasswordComponent,
        TokenResetPasswordComponent,
        CustomPageComponent,
        CookieComponent,
        LoginOAuthComponent,
    ],
    providers: [
        NotificationService,
        AuthService,
        AuthGuard,
        SettingsService,
        IdentityService,
        StatusService,
        LandingPageService,
        ConfirmationService,
        ImageService,
        CookieService,
        JobService,
        IssuesService,
        PlexTvService,
        CustomPageService,
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
