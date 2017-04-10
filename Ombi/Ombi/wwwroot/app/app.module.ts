import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';

import { RouterModule, Routes } from '@angular/router';
import { HttpModule } from '@angular/http';

import { InfiniteScrollModule } from 'angular2-infinite-scroll/angular2-infinite-scroll'

import { SearchComponent } from './search/search.component';
import { RequestComponent } from './requests/request.component';
import { PageNotFoundComponent } from './errors/not-found.component';

// Services
import { SearchService } from './services/search.service';
import { RequestService } from './services/request.service';
import { NotificationService } from './services/notification.service';

// Modules
import { SettingsModule } from './settings/settings.module';

import { ButtonModule } from 'primeng/primeng';
import { GrowlModule } from 'primeng/components/growl/growl';
import { DataTableModule, SharedModule } from 'primeng/primeng';

const routes: Routes = [
    { path: '*', component: PageNotFoundComponent },
    { path: 'search', component: SearchComponent },
    { path: 'requests', component: RequestComponent },
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
        InfiniteScrollModule
    ],
    declarations: [
        AppComponent,
        PageNotFoundComponent,
        SearchComponent,
        RequestComponent
    ],
    providers: [
        SearchService,
        RequestService,
        NotificationService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
