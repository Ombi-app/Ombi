import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { TranslateModule } from "@ngx-translate/core";
import { TruncateModule } from "@yellowspot/ng-truncate";

import { IssuesReportComponent } from "./issues-report.component";

import { InputSwitchModule, SidebarModule } from "primeng/primeng";

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
  ],
  exports: [
      TranslateModule,
      CommonModule,
      FormsModule,
      SidebarModule,
      IssuesReportComponent,
      TruncateModule,
      InputSwitchModule,
    ],
})
export class SharedModule {}
