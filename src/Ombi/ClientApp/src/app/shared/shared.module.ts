import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { TranslateModule } from "@ngx-translate/core";
import { TruncateModule } from "@yellowspot/ng-truncate";
import { MomentModule } from "ngx-moment";

import { IssuesReportComponent } from "./issues-report.component";

import { SidebarModule } from "primeng/sidebar";
import { InputSwitchModule } from "primeng/inputswitch";

import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import {MatMenuModule} from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTreeModule } from '@angular/material/tree';
  import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatChipsModule } from "@angular/material/chips";
import { MatDialogModule } from "@angular/material/dialog";
import { MatExpansionModule } from "@angular/material/expansion";
import { MatInputModule } from "@angular/material/input";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTabsModule } from "@angular/material/tabs";
import { EpisodeRequestComponent } from "./episode-request/episode-request.component";

@NgModule({
  declarations: [
    IssuesReportComponent,
    EpisodeRequestComponent,
  ],
  imports: [
    SidebarModule,
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
  entryComponents: [
    EpisodeRequestComponent,
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
