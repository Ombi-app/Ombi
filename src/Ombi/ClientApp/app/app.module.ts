import {CommonModule, PlatformLocation} from "@angular/common";
import {HttpClient, HttpClientModule} from "@angular/common/http";
import {NgModule} from "@angular/core";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {HttpModule} from "@angular/http";
import {MatButtonModule, MatCardModule, MatInputModule, MatTabsModule} from "@angular/material";
import {BrowserModule} from "@angular/platform-browser";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {RouterModule, Routes} from "@angular/router";

// Third Party
//import { DragulaModule, DragulaService } from 'ng2-dragula/ng2-dragula';
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { GrowlModule } from "primeng/components/growl/growl";
import { ButtonModule, CaptchaModule,ConfirmationService, ConfirmDialogModule, DataTableModule,DialogModule, SharedModule, TooltipModule } from "primeng/primeng";

// Components
import { AppComponent } from "./app.component";

import { PageNotFoundComponent } from "./errors/not-found.component";
import { LandingPageComponent } from "./landingpage/landingpage.component";
import { LoginComponent } from "./login/login.component";
import { ResetPasswordComponent } from "./login/resetpassword.component";
import { TokenResetPasswordComponent } from "./login/tokenresetpassword.component";

// Services
import { AuthGuard } from "./auth/auth.guard";
import { AuthModule } from "./auth/auth.module";
import { AuthService } from "./auth/auth.service";
import { IdentityService } from "./services";
import { ImageService } from "./services";
import { LandingPageService } from "./services";
import { NotificationService } from "./services";
import { RequestService } from "./services";
import { SettingsService } from "./services";
import { StatusService } from "./services";

// Modules
import { RequestsModule } from "./requests/requests.module";
import { SearchModule } from "./search/search.module";
import { SettingsModule } from "./settings/settings.module";
import { UserManagementModule } from "./usermanagement/usermanagement.module";
import { WizardModule } from "./wizard/wizard.module";

const routes: Routes = [
    { path: "*", component: PageNotFoundComponent },
    { path: "", redirectTo: "/search", pathMatch: "full" },

    //{ path: 'requests-grid', component: RequestGridComponent },
    { path: "login", component: LoginComponent },
    { path: "login/:landing", component: LoginComponent },
    { path: "reset", component: ResetPasswordComponent },
    { path: "token", component: TokenResetPasswordComponent },
    { path: "landingpage", component: LandingPageComponent },
];

// AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient, platformLocation: PlatformLocation) {
    const base = platformLocation.getBaseHrefFromDOM();
    if (base.length > 1) {
        return new TranslateHttpLoader(http, `${base}/translations/`, ".json");
    }
    return new TranslateHttpLoader(http, "/translations/", ".json");
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
        SettingsModule,
        DataTableModule,
        SharedModule,
        AuthModule,
        WizardModule,
        SearchModule,
        DialogModule,
        MatButtonModule,
        NgbModule.forRoot(),
        MatCardModule,
        MatInputModule,
        MatTabsModule,
        ReactiveFormsModule,
        UserManagementModule,
        RequestsModule,
        CaptchaModule,
        TooltipModule,
        ConfirmDialogModule,
        CommonModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient, PlatformLocation],
            },
        }),
    ],
    declarations: [
        AppComponent,
        PageNotFoundComponent,
        LoginComponent,
        LandingPageComponent,
        ResetPasswordComponent,
        TokenResetPasswordComponent,
    ],
    providers: [
        RequestService,
        NotificationService,
        AuthService,
        AuthGuard,
        SettingsService,
        IdentityService,
        StatusService,
        LandingPageService,
        ConfirmationService,
        ImageService,
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
