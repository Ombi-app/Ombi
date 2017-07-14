import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MdButtonModule, MdCardModule, MdInputModule, MdTabsModule } from '@angular/material';
import { RouterModule, Routes } from '@angular/router';
import { HttpModule } from '@angular/http';

// Third Party
import { ButtonModule, DialogModule, CaptchaModule } from 'primeng/primeng';
import { GrowlModule } from 'primeng/components/growl/growl';
import { DataTableModule, SharedModule } from 'primeng/primeng';
//import { DragulaModule, DragulaService } from 'ng2-dragula/ng2-dragula';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

// Components
import { AppComponent } from './app.component';

import { LoginComponent } from './login/login.component';
import { ResetPasswordComponent } from './login/resetpassword.component';
import { TokenResetPasswordComponent } from './login/tokenresetpassword.component';
import { LandingPageComponent } from './landingpage/landingpage.component';
import { PageNotFoundComponent } from './errors/not-found.component';

// Services 
import { RequestService } from './services/request.service';
import { NotificationService } from './services/notification.service';
import { SettingsService } from './services/settings.service';
import { AuthService } from './auth/auth.service';
import { AuthGuard } from './auth/auth.guard';
import { AuthModule } from './auth/auth.module';
import { IdentityService } from './services/identity.service';
import { StatusService } from './services/status.service';


// Modules
import { SettingsModule } from './settings/settings.module';
import { WizardModule } from './wizard/wizard.module';
import { SearchModule } from './search/search.module';
import { UserManagementModule } from './usermanagement/usermanagement.module';
import { RequestsModule } from './requests/requests.module';

const routes: Routes = [
    { path: '*', component: PageNotFoundComponent },
    { path: '', redirectTo: '/search', pathMatch: 'full' },
   
    //{ path: 'requests-grid', component: RequestGridComponent },
    { path: 'login', component: LoginComponent },
    { path: 'reset', component: ResetPasswordComponent },
    { path: 'landingpage', component: LandingPageComponent }
];

@NgModule({
    imports: [
        RouterModule.forRoot(routes),
        BrowserModule,
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
        MdButtonModule,
        NgbModule.forRoot(),
        //DragulaModule,
        MdCardModule,
        MdInputModule,
        MdTabsModule,
        ReactiveFormsModule,
        UserManagementModule,
        RequestsModule,
        CaptchaModule
    ],
    declarations: [
        AppComponent,
        PageNotFoundComponent,
        LoginComponent,
        LandingPageComponent,
        ResetPasswordComponent,
        TokenResetPasswordComponent
    ],
    providers: [
        RequestService,
        NotificationService,
        AuthService,
        AuthGuard,
        SettingsService,
        IdentityService,
        StatusService,
    //DragulaService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
