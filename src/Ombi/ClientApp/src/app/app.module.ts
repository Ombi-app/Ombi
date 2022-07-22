import { APP_BASE_HREF, CommonModule, PlatformLocation } from "@angular/common";
import { CustomPageService, ImageService, RequestService, SettingsService } from "./services";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from "@angular/common/http";
import { IdentityService, IssuesService, JobService, MessageService, PlexTvService, SearchService, StatusService } from "./services";
import { RouterModule, Routes } from "@angular/router";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";

import { AppComponent } from "./app.component";
import { AuthGuard } from "./auth/auth.guard";
import { AuthService } from "./auth/auth.service";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { BrowserModule } from "@angular/platform-browser";
import { ButtonModule } from "primeng/button";
import { CUSTOMIZATION_INITIALIZER } from "./state/customization/customization-initializer";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { CookieComponent } from "./auth/cookie.component";
import { CookieService } from "ng2-cookies";
import { CustomPageComponent } from "./custompage/custompage.component";
import { CustomizationState } from "./state/customization/customization.state";
import { DataViewModule } from "primeng/dataview";
import { DialogModule } from "primeng/dialog";
import { FEATURES_INITIALIZER } from "./state/features/features-initializer";
import { FeatureState } from "./state/features";
import { JwtModule } from "@auth0/angular-jwt";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { LandingPageService } from "./services";
import { LayoutModule } from '@angular/cdk/layout';
import { LoginComponent } from "./login/login.component";
import { LoginOAuthComponent } from "./login/loginoauth.component";
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from "@angular/material/chips";
import { MatDialogModule } from "@angular/material/dialog";
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from "@angular/material/input";
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from "@angular/material/menu";
import { MatNativeDateModule } from '@angular/material/core';
import { MatPaginatorI18n } from "./localization/MatPaginatorI18n";
import { MatPaginatorIntl } from "@angular/material/paginator";
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from "@angular/material/tabs";
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from "@angular/material/tooltip";
import { MyNavComponent } from './my-nav/my-nav.component';
import { NavSearchComponent } from "./my-nav/nav-search.component";
import { NgModule } from "@angular/core";
import { NgxsModule } from '@ngxs/store';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';
import { NotificationService } from "./services";
import { OverlayModule } from "@angular/cdk/overlay";
import { OverlayPanelModule } from "primeng/overlaypanel";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { RemainingRequestsComponent } from "./shared/remaining-requests/remaining-requests.component";
import { ResetPasswordComponent } from "./login/resetpassword.component";
import { SearchV2Service } from "./services/searchV2.service";
import { SidebarModule } from "primeng/sidebar";
import { SignalRNotificationService } from "./services/signlarnotification.service";
import { StorageService } from "./shared/storage/storage-service";
import { TokenResetPasswordComponent } from "./login/tokenresetpassword.component";
import { TooltipModule } from "primeng/tooltip";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { TranslateService } from "@ngx-translate/core";
import { UnauthorizedInterceptor } from "./auth/unauthorized.interceptor";
import { ImageBackgroundComponent } from "./components/";
import { environment } from "../environments/environment";

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
    { loadChildren: () => import("./unsubscribe/unsubscribe.module").then(m => m.UnsubscribeModule), path: "unsubscribe" },
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
        MatCardModule,
        MatTooltipModule,
        MatMenuModule,
        MatInputModule,
        MatTabsModule,
        MatChipsModule,
        MatDialogModule,
        ReactiveFormsModule,
        MatAutocompleteModule,
        TooltipModule,
        ConfirmDialogModule,
        OverlayPanelModule,
        CommonModule,
        OverlayModule,
        MatCheckboxModule,
        MatProgressSpinnerModule,
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
        MatNativeDateModule, MatIconModule, MatSidenavModule, MatListModule, MatToolbarModule, LayoutModule, MatSlideToggleModule,
        NgxsModule.forRoot([CustomizationState, FeatureState], {
            developmentMode: !environment.production,
        }),
        ...environment.production ? [] :
        [
            NgxsReduxDevtoolsPluginModule.forRoot(),
        ],
        ImageBackgroundComponent
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
        SignalRNotificationService,
        FEATURES_INITIALIZER,
        CUSTOMIZATION_INITIALIZER,
        {
            provide: APP_BASE_HREF,
            useValue: window["baseHref"]
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: UnauthorizedInterceptor,
            multi: true
        },
        {
            provide: MatPaginatorIntl, deps: [TranslateService],
            useFactory: (translateService: TranslateService) => new MatPaginatorI18n(translateService).getPaginatorIntl()
        },
       ],
    bootstrap: [AppComponent],
})
export class AppModule { }
