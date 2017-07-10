import { NgModule, } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { SearchComponent } from './search.component';
import { MovieSearchComponent } from './moviesearch.component';
import { TvSearchComponent } from './tvsearch.component';
import { SeriesInformationComponent } from './seriesinformation.component';

import { SearchService } from '../services/search.service';
import { RequestService } from '../services/request.service';

import { AuthGuard } from '../auth/auth.guard';

const routes: Routes = [
    { path: 'search', component: SearchComponent, canActivate: [AuthGuard] },
    { path: 'search/show/:id', component: SeriesInformationComponent, canActivate: [AuthGuard] },
];

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        RouterModule.forChild(routes),
        NgbModule.forRoot(),
    ],
    declarations: [
        SearchComponent,
        MovieSearchComponent,
        TvSearchComponent,
        SeriesInformationComponent
    ],
    exports: [
        RouterModule
    ],
    providers: [
        SearchService,
        RequestService
    ],
})
export class SearchModule { }