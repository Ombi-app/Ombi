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
  import {  MatCardModule, MatInputModule, MatTabsModule, MatAutocompleteModule, MatCheckboxModule, MatExpansionModule, MatDialogModule, MatSnackBarModule } from "@angular/material";

@NgModule({
  declarations: [
    IssuesReportComponent,
  ],
  imports: [
    SidebarModule,
    FormsModule,
    CommonModule,
    InputSwitchModule,
    TruncateModule,
    MomentModule,
    MatCardModule,
    MatAutocompleteModule,
    MatInputModule,
    MatTabsModule,
    MatButtonModule,
    MatNativeDateModule,
    MatIconModule, 
    MatSidenavModule, 
    MatListModule, 
    MatToolbarModule,
    MatCheckboxModule,
    MatExpansionModule,
    MatDialogModule,
    MatSnackBarModule,
  ],
  exports: [
      TranslateModule,
      CommonModule,
      FormsModule,
      SidebarModule,
      IssuesReportComponent,
      TruncateModule,
      InputSwitchModule,
      MomentModule,MatCardModule,
      MatInputModule,
      MatTabsModule,
      MatButtonModule,
      MatNativeDateModule,
      MatIconModule, 
      MatSnackBarModule,
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
