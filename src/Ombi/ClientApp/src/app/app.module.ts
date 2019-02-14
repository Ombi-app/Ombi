import { CommonModule, PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient, HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpModule } from "@angular/http";
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
import {
    ButtonModule, CaptchaModule, ConfirmationService, ConfirmDialogModule, DataTableModule, DialogModule, OverlayPanelModule, SharedModule, SidebarModule,
    TooltipModule
} from "primeng/primeng";

import {
    MatButtonModule, MatNativeDateModule, MatIconModule, MatSidenavModule, MatListModule, MatToolbarModule, MatAutocompleteModule, MatCheckboxModule
} from '@angular/material';
import { MatCardModule, MatInputModule, MatTabsModule } from "@angular/material";

import { MDBBootstrapModule, CardsFreeModule, NavbarModule } from "angular-bootstrap-md";

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
import { ImageService, SettingsService, CustomPageService } from "./services";
import { LandingPageService } from "./services";
import { NotificationService } from "./services";
import { IssuesService, JobService, PlexTvService, StatusService, SearchService, IdentityService } from "./services";
import { MyNavComponent } from './my-nav/my-nav.component';
import { LayoutModule } from '@angular/cdk/layout';
import { SearchV2Service } from "./services/searchV2.service";
import { NavSearchComponent } from "./my-nav/nav-search.component";

const routes: Routes = [
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
    { loadChildren: "./discover/discover.module#DiscoverModule", path: "discover" },
    { loadChildren: "./issues/issues.module#IssuesModule", path: "issues" },
    { loadChildren: "./settings/settings.module#SettingsModule", path: "Settings" },
    { loadChildren: "./wizard/wizard.module#WizardModule", path: "Wizard" },
    { loadChildren: "./usermanagement/usermanagement.module#UserManagementModule", path: "usermanagement" },
    { loadChildren: "./requests/requests.module#RequestsModule", path: "requests" },
    { loadChildren: "./search/search.module#SearchModule", path: "search" },
    { loadChildren: "./recentlyAdded/recentlyAdded.module#RecentlyAddedModule", path: "recentlyadded" },
    { loadChildren: "./vote/vote.module#VoteModule", path: "vote" },
    { loadChildren: "./media-details/media-details.module#MediaDetailsModule", path: "details" },
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
export function baseurlFact() {
    return "/" + localStorage.getItem("baseUrl");
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
        NavbarModule,
        NgbModule.forRoot(),
        MatCardModule,
        MatInputModule,
        MatTabsModule,
        ReactiveFormsModule,
        MatAutocompleteModule,
        CaptchaModule,
        TooltipModule,
        ConfirmDialogModule,
        OverlayPanelModule,
        CommonModule,
        CardsFreeModule,
        MatCheckboxModule,
        MDBBootstrapModule.forRoot(),
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
        MatNativeDateModule, MatIconModule, MatSidenavModule, MatListModule, MatToolbarModule, LayoutModule,
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
        MyNavComponent,
        NavSearchComponent,
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
        CustomPageService,
        CookieService,
        JobService,
        IssuesService,
        PlexTvService,
        SearchService,
        SearchV2Service,
        {
            provide: APP_BASE_HREF, 
            useFactory: baseurlFact           
        }    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
