import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AuthService } from "../../../auth/auth.service";
import { IIssues, IIssueSettings, IssueStatus } from "../../../interfaces";
import { SettingsService } from "../../../services";


export interface IssuesDetailsGroupData {
    issues: IIssues[];
    title: string;
  }

@Component({
    selector: "issues-details-group",
    templateUrl: "details-group.component.html",
    styleUrls: ["details-group.component.scss"],
})
export class DetailsGroupComponent implements OnInit {

    public isAdmin: boolean;
    public IssueStatus = IssueStatus;
    public settings: IIssueSettings;
    public get hasRequest(): boolean {
        return this.data.issues.some(x => x.requestId);
    }

 constructor(public dialogRef: MatDialogRef<DetailsGroupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: IssuesDetailsGroupData,
    private authService: AuthService,  private settingsService: SettingsService) { }

    public ngOnInit() {
        this.isAdmin = this.authService.hasRole("Admin") || this.authService.hasRole("PowerUser");
        this.settingsService.getIssueSettings().subscribe(x => this.settings = x);
    }

    public close() {
        this.dialogRef.close();
      }

      public navToRequest() {
          var issue = this.data.issues.filter(x => {
              return x.requestId;
          })[0];

          // close dialog and tell calling component to navigate
      }

}
