import { CommonModule, PlatformLocation, APP_BASE_HREF } from "@angular/common";
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";

import { JwtModule } from "@auth0/angular-jwt";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { CookieService } from "ng2-cookies";

import { ButtonModule } from "primeng/button";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { DataViewModule } from "primeng/dataview";
import { DialogModule } from "primeng/dialog";
import { OverlayPanelModule } from "primeng/overlaypanel";
import { TooltipModule } from "primeng/tooltip";
import { SidebarModule } from "primeng/sidebar";

import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from "@angular/material/card";
import { MatInputModule } from "@angular/material/input";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTabsModule } from "@angular/material/tabs";
import { MatTooltipModule } from "@angular/material/tooltip";


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
import { ImageService, SettingsService, CustomPageService, RequestService } from "./services";
import { LandingPageService } from "./services";
import { NotificationService } from "./services";
import { IssuesService, JobService, PlexTvService, StatusService, SearchService, IdentityService, MessageService } from "./services";
import { MyNavComponent } from './my-nav/my-nav.component';
import { LayoutModule } from '@angular/cdk/layout';
import { SearchV2Service } from "./services/searchV2.service";
import { NavSearchComponent } from "./my-nav/nav-search.component";
import { OverlayModule } from "@angular/cdk/overlay";
import { StorageService } from "./shared/storage/storage-service";
import { SignalRNotificationService } from "./services/signlarnotification.service";
import { MatMenuModule } from "@angular/material/menu";
import { RemainingRequestsComponent } from "./shared/remaining-requests/remaining-requests.component";
import { UnauthorizedInterceptor } from "./auth/unauthorized.interceptor";
import { FilterService } from "./discover/services/filter-service";

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
    { loadChildren: () => import("./calendar/calendar.module").then(m => m.CalendarModule), path: "calendar" },
    { loadChildren: () => import("./discover/discover.module").then(m => m.DiscoverModule), path: "discover" },
    { loadChildren: () => import("./issues/issues.module").then(m => m.IssuesModule), path: "issues" },
    { loadChildren: () => import("./settings/settings.module").then(m => m.SettingsModule), path: "Settings" },
    { loadChildren: () => import("./wizard/wizard.module").then(m => m.WizardModule), path: "Wizard" },
    { loadChildren: () => import("./usermanagement/usermanagement.module").then(m => m.UserManagementModule), path: "usermanagement" },
    // { loadChildren: () => import("./requests/requests.module").then(m => m.RequestsModule), path: "requestsOld" },
    { loadChildren: () => import("./requests-list/requests-list.module").then(m => m.RequestsListModule), path: "requests-list" },
    { loadChildren: () => import("./vote/vote.module").then(m => m.VoteModule), path: "vote" },
    { loadChildren: () => import("./media-details/media-details.module").then(m => m.MediaDetailsModule), path: "details" },
    { loadChildren: () => import("./user-preferences/user-preferences.module").then(m => m.UserPreferencesModule), path: "user-preferences" },
];



// AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient, platformLocation: PlatformLocation) {
    // const base = getBaseLocation();
    const base = window["baseHref"];
    const version = Math.floor(Math.random() * 999999999);
    if (base !== null && base.length > 1) {
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
        ButtonModule,
        FormsModule,
        DataViewModule,
        MatSnackBarModule,
        MatSnackBarModule,
        DialogModule,
        MatButtonModule,
        NavbarModule,
        MatCardModule,
        MatTooltipModule,
        MatMenuModule,
        MatInputModule,
        MatTabsModule,
        ReactiveFormsModule,
        MatAutocompleteModule,
        TooltipModule,
        ConfirmDialogModule,
        OverlayPanelModule,
        CommonModule,
        CardsFreeModule,
        OverlayModule,
        MatCheckboxModule,
        MatProgressSpinnerModule,
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
        MatNativeDateModule, MatIconModule, MatSidenavModule, MatListModule, MatToolbarModule, LayoutModule, MatSlideToggleModule
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
        RemainingRequestsComponent,
    ],

    providers: [
        NotificationService,
        AuthService,
        AuthGuard,
        SettingsService,
        IdentityService,
        StatusService,
        LandingPageService,
        ImageService,
        CustomPageService,
        CookieService,
        JobService,
        IssuesService,
        PlexTvService,
        SearchService,
        SearchV2Service,
        MessageService,
        StorageService,
        RequestService,
        FilterService,
        SignalRNotificationService,
        {
            provide: APP_BASE_HREF,
            useValue: window["baseHref"]
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: UnauthorizedInterceptor,
            multi: true
        }
       ],
    bootstrap: [AppComponent],
})
export class AppModule { }
