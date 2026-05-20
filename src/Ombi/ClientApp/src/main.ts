// Main
import "jquery";

import "bootstrap/dist/js/bootstrap";


import { environment } from "./environments/environment";

import "./polyfills";

import { bootstrapApplication } from "@angular/platform-browser";
import { AppComponent } from "./app/app.component";
import { importProvidersFrom } from "@angular/core";
import { RouterModule } from "@angular/router";
import { withInterceptorsFromDi } from "@angular/common/http";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { TranslateModule, TranslateLoader } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { JwtModule } from "@auth0/angular-jwt";
import { NgxsModule } from '@ngxs/store';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';
import { MatPaginatorIntl } from "@angular/material/paginator";
import { MatPaginatorI18n } from "./app/localization/MatPaginatorI18n";
import { TranslateService } from "@ngx-translate/core";
import { APP_BASE_HREF, PlatformLocation } from "@angular/common";
import { HTTP_INTERCEPTORS, HttpClient, provideHttpClient, withFetch } from "@angular/common/http";
import { UnauthorizedInterceptor } from "./app/auth/unauthorized.interceptor";

// Angular Material modules
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from "@angular/material/card";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatMenuModule } from "@angular/material/menu";
import { MatInputModule } from "@angular/material/input";
import { MatTabsModule } from "@angular/material/tabs";
import { MatChipsModule } from "@angular/material/chips";
import { MatDialogModule } from "@angular/material/dialog";
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatNativeDateModule } from '@angular/material/core';
import { LayoutModule } from '@angular/cdk/layout';
import { OverlayModule } from "@angular/cdk/overlay";

// PrimeNG modules
import { ButtonModule } from "primeng/button";
import { DataViewModule } from "primeng/dataview";
import { DialogModule } from "primeng/dialog";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { OverlayPanelModule } from "primeng/overlaypanel";
import { SidebarModule } from "primeng/sidebar";
import { TooltipModule } from "primeng/tooltip";

// Services
import { NotificationService } from "./app/services";
import { AuthService } from "./app/auth/auth.service";
import { AuthGuard } from "./app/auth/auth.guard";
import { SettingsService } from "./app/services";
import { IdentityService } from "./app/services";
import { StatusService } from "./app/services";
import { LandingPageService } from "./app/services";
import { ImageService } from "./app/services";
import { CustomPageService } from "./app/services";
import { JobService } from "./app/services";
import { IssuesService } from "./app/services";
import { IssuesV2Service } from "./app/services/issuesv2.service";
import { WizardService } from "./app/wizard/services/wizard.service";
import { PlexTvService, PlexService, TesterService, RadarrService, EmbyService, JellyfinService, ValidationService } from "./app/services";
import { SearchService } from "./app/services";
import { SearchV2Service } from "./app/services/searchV2.service";
import { MessageService } from "./app/services";
import { StorageService } from "./app/shared/storage/storage-service";
import { RequestService } from "./app/services";
import { SonarrService } from "./app/services";
import { LidarrService } from "./app/services";
import { SignalRNotificationService } from "./app/services/signlarnotification.service";

// State
import { CustomizationState } from "./app/state/customization/customization.state";
import { FeatureState } from "./app/state/features";
import { SonarrSettingsState } from "./app/state/sonarr";
import { RadarrSettingsState } from "./app/state/radarr";
import { FEATURES_INITIALIZER } from "./app/state/features/features-initializer";
import { SONARR_INITIALIZER } from "./app/state/sonarr/sonarr-initializer";
import { CUSTOMIZATION_INITIALIZER } from "./app/state/customization/customization-initializer";
import { RADARR_INITIALIZER } from "./app/state/radarr/radarr-initializer";

// Routes
import { routes } from "./app/app.routes";

// Factory functions
export function HttpLoaderFactory(http: HttpClient, platformLocation: PlatformLocation) {
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

if (environment.production) {
    // enableProdMode() is a no-op in Angular 15+ — production mode is handled automatically.
  }

bootstrapApplication(AppComponent, {
    providers: [
        // Core Angular providers
        importProvidersFrom(
            RouterModule.forRoot(routes),
            BrowserAnimationsModule,
            FormsModule,
            ReactiveFormsModule,
            CommonModule,
            TranslateModule.forRoot({
                loader: {
                    provide: TranslateLoader,
                    useFactory: HttpLoaderFactory,
                    deps: [HttpClient, PlatformLocation],
                },
            }),
            JwtModule.forRoot({
                config: {
                    tokenGetter: JwtTokenGetter,
                },
            }),
            NgxsModule.forRoot([CustomizationState, FeatureState, SonarrSettingsState, RadarrSettingsState], {
                developmentMode: !environment.production,
            }),
            ...environment.production ? [] : [NgxsReduxDevtoolsPluginModule.forRoot()],
            // Angular Material modules
            MatSnackBarModule,
            MatButtonModule,
            MatCardModule,
            MatTooltipModule,
            MatMenuModule,
            MatInputModule,
            MatTabsModule,
            MatChipsModule,
            MatDialogModule,
            MatAutocompleteModule,
            MatCheckboxModule,
            MatProgressSpinnerModule,
            MatProgressBarModule,
            MatIconModule,
            MatSidenavModule,
            MatListModule,
            MatToolbarModule,
            MatSlideToggleModule,
            MatNativeDateModule,
            LayoutModule,
            OverlayModule,
            // PrimeNG modules
            ButtonModule,
            DataViewModule,
            DialogModule,
            ConfirmDialogModule,
            OverlayPanelModule,
            SidebarModule,
            TooltipModule
        ),
        
        // HTTP Client with Fetch API support and DI interceptors
        provideHttpClient(withFetch(), withInterceptorsFromDi()),
        
        // Services
        NotificationService,
        AuthService,
        AuthGuard,
        SettingsService,
        IdentityService,
        StatusService,
        LandingPageService,
        ImageService,
        CustomPageService,
        JobService,
        IssuesService,
        IssuesV2Service,
        WizardService,
        PlexTvService,
        PlexService,
        TesterService,
        RadarrService,
        EmbyService,
        JellyfinService,
        ValidationService,
        SearchService,
        SearchV2Service,
        MessageService,
        StorageService,
        RequestService,
        SonarrService,
        LidarrService,
        SignalRNotificationService,
        FEATURES_INITIALIZER,
        SONARR_INITIALIZER,
        CUSTOMIZATION_INITIALIZER,
        RADARR_INITIALIZER,
        
        // Configuration providers
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
            provide: MatPaginatorIntl, 
            deps: [TranslateService],
            useFactory: (translateService: TranslateService) => new MatPaginatorI18n(translateService).getPaginatorIntl()
        }
    ]
}).catch(err => console.log(err));
