import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { TranslateModule } from "@ngx-translate/core";
import { TruncateModule } from "@yellowspot/ng-truncate";
import { MomentModule } from "ngx-moment";

import { IssuesReportComponent } from "./issues-report.component";

import { InputSwitchModule, SidebarModule } from "primeng/primeng";

import {
  MatButtonModule, MatNativeDateModule, MatIconModule, MatSidenavModule, MatListModule, MatToolbarModule, MatTooltipModule} from '@angular/material';
  import {  MatCardModule, MatInputModule, MatTabsModule, MatAutocompleteModule, MatCheckboxModule, MatExpansionModule, MatDialogModule, MatProgressSpinnerModule,
    MatChipsModule } from "@angular/material";
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
    MatSidenavModule, 
    MatListModule, 
    MatToolbarModule,
    MatCheckboxModule,
    TranslateModule,
    MatExpansionModule,
    MatDialogModule,
    MatTooltipModule,
  ],
  entryComponents: [
    EpisodeRequestComponent
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
      MomentModule,MatCardModule,
      MatInputModule,
      MatTabsModule,
      MatChipsModule,
      MatButtonModule,
      MatNativeDateModule,
      MatIconModule, 
      MatSidenavModule, 
      MatListModule, 
      MatToolbarModule,
      MatTooltipModule,
      MatAutocompleteModule,
      MatCheckboxModule,
      MatExpansionModule,
      MatDialogModule,
    ],
})
export class SharedModule {}
