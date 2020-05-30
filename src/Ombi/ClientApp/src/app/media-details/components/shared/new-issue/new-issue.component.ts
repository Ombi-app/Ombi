import { Component, Inject, OnInit } from "@angular/core";
import { IDenyDialogData, IIssueDialogData } from "../interfaces/interfaces";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MessageService, IssuesService } from "../../../../services";
import { IIssues, IIssueCategory, IssueStatus, RequestType } from "../../../../interfaces";
import { TranslateService } from "@ngx-translate/core";

@Component({
    selector: "new-issue",
    templateUrl: "./new-issue.component.html",
})
export class NewIssueComponent implements OnInit {
   
    public issue: IIssues;
    public issueCategories: IIssueCategory[];

    constructor(
        public dialogRef: MatDialogRef<NewIssueComponent>,
        @Inject(MAT_DIALOG_DATA) public data: IIssueDialogData,
        private issueService: IssuesService,
        public messageService: MessageService,
        private translate: TranslateService) {
            debugger;
            this.issue = {
                subject: "",
                description: "",
                issueCategory: { value: "", id: 0 },
                status: IssueStatus.Pending,
                resolvedDate: undefined,
                id: undefined,
                issueCategoryId: 0,
                comments: [],
                requestId: data.requestId,
                requestType: data.requestType,
                title: data.title,
                providerId: data.providerId,
                userReported: undefined,
            };
        }

        public async ngOnInit(): Promise<void> {
            this.issueCategories = await this.issueService.getCategories().toPromise();
        }   

        public async createIssue() { 
            const result = await this.issueService.createIssue(this.issue).toPromise();
            if(result) {
                this.messageService.send(this.translate.instant("Issues.IssueDialog.IssueCreated"));
            }
        }

        public onNoClick(): void {
            this.dialogRef.close();
            delete this.issue;
          }
}
