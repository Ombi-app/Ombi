import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { AdminRequestDialogComponent } from "./admin-request-dialog/admin-request-dialog.component";
import { AdvancedSearchDialogComponent } from "./advanced-search-dialog/advanced-search-dialog.component";
import { CommonModule } from "@angular/common";
import { DetailsGroupComponent } from "../issues/components/details-group/details-group.component";
import { EpisodeRequestComponent } from "./episode-request/episode-request.component";
import { GenreSelectComponent } from "./components/genre-select/genre-select.component";
import { InputSwitchModule } from "primeng/inputswitch";
import { IssuesReportComponent } from "./issues-report.component";
import { KeywordSearchComponent } from "./components/keyword-search/keyword-search.component";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatChipsModule } from "@angular/material/chips";
import { MatDialogModule } from "@angular/material/dialog";
import { MatExpansionModule } from "@angular/material/expansion";
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from "@angular/material/input";
import { MatListModule } from '@angular/material/list';
import {MatMenuModule} from '@angular/material/menu';
import { MatNativeDateModule } from '@angular/material/core';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import {MatRadioModule} from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from "@angular/material/tabs";
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTreeModule } from '@angular/material/tree';
import { MomentModule } from "ngx-moment";
import { NgModule } from "@angular/core";
import { SidebarModule } from "primeng/sidebar";
import { TheMovieDbService } from "../services";
import { TranslateModule } from "@ngx-translate/core";
import { TruncateModule } from "@yellowspot/ng-truncate";

@NgModule({
  declarations: [
    IssuesReportComponent,
    EpisodeRequestComponent,
    DetailsGroupComponent,
    AdminRequestDialogComponent,
    AdvancedSearchDialogComponent,
    KeywordSearchComponent,
    GenreSelectComponent,
  ],
  imports: [
    SidebarModule,
    ReactiveFormsModule,
    FormsModule,
    CommonModule,
    InputSwitchModule,
    TruncateModule,
    MomentModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatAutocompleteModule,
    MatInputModule,
    MatTabsModule,
    MatRadioModule,
    MatButtonModule,
    MatNativeDateModule,
    MatChipsModule,
    MatIconModule,
    MatMenuModule,
    MatSidenavModule,
    MatListModule,
    MatToolbarModule,
    MatCheckboxModule,
    TranslateModule,
    MatExpansionModule,
    MatDialogModule,
    MatTooltipModule,
    MatSelectModule,
    MatPaginatorModule,
    MatSortModule,
    MatTreeModule,
    MatStepperModule,
    MatSnackBarModule,
  ],
  exports: [
      TranslateModule,
      CommonModule,
      FormsModule,
      TranslateModule,
      SidebarModule,
      MatProgressSpinnerModule,
      IssuesReportComponent,
      EpisodeRequestComponent,
      AdminRequestDialogComponent,
      AdvancedSearchDialogComponent,
      GenreSelectComponent,
      KeywordSearchComponent,
      DetailsGroupComponent,
      TruncateModule,
      InputSwitchModule,
      MatTreeModule,
      MomentModule,MatCardModule,
      MatInputModule,
      MatTabsModule,
      MatChipsModule,
      MatButtonModule,
      MatNativeDateModule,
      MatIconModule,
      MatMenuModule,
      MatSnackBarModule,
      MatSidenavModule,
      MatSelectModule,
      MatListModule,
      MatToolbarModule,
      MatTooltipModule,
      MatAutocompleteModule,
      MatCheckboxModule,
      MatExpansionModule,
      MatDialogModule,
      MatTableModule,
      MatPaginatorModule,
      MatSortModule,
      MatStepperModule,
      MatSlideToggleModule,
    ],
})
export class SharedModule {}
