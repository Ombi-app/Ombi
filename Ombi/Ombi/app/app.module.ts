import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';

import { RouterModule, Routes } from '@angular/router';
import { HttpModule } from '@angular/http';

import { SearchComponent } from './search/search.component';
import { PageNotFoundComponent } from './errors/not-found.component';

// Services
import { SearchService } from './services/search.service';

import { ButtonModule } from 'primeng/primeng';
import { MenubarModule } from 'primeng/components/menubar/menubar';
import { GrowlModule } from 'primeng/components/growl/growl';

const routes: Routes = [
    { path: '*', component: PageNotFoundComponent },
    { path: 'search', component: SearchComponent }
];

@NgModule({
    imports: [
        RouterModule.forRoot(routes),
        BrowserModule,
        BrowserAnimationsModule,
        HttpModule,
        MenubarModule,
        GrowlModule,
        ButtonModule,
        FormsModule
    ],
    declarations: [
        AppComponent,
        PageNotFoundComponent,
        SearchComponent
    ],
    providers: [
        SearchService
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
