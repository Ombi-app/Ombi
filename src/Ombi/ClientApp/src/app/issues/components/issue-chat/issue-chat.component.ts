import { Component, Inject, OnInit } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { AuthService } from "../../../auth/auth.service";
import { ILocalUser } from "../../../auth/IUserLogin";
import { IIssuesChat, IIssueSettings, IssueStatus } from "../../../interfaces";
import { IssuesService, SettingsService } from "../../../services";
import { ChatMessages, ChatType, ChatBoxComponent } from "../../../shared/chat-box/chat-box.component";


export interface ChatData {
    issueId: number;
    title: string;
  }

@Component({
    standalone: true,
    selector: "issue-chat",
    templateUrl: "issue-chat.component.html",
    styleUrls: ["issue-chat.component.scss"],
    imports: [
        CommonModule,
        MatDialogModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        MatTooltipModule,
        TranslateModule,
        ChatBoxComponent
    ]
})
export class IssueChatComponent implements OnInit {

    public isAdmin: boolean;
    public comments: IIssuesChat[] = [];
    public IssueStatus = IssueStatus;
    public settings: IIssueSettings;
    public messages: ChatMessages[] = [];

    public loaded: boolean;

    private user: ILocalUser;


 constructor(public dialogRef: MatDialogRef<IssueChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ChatData,
    private authService: AuthService,  private settingsService: SettingsService,
    private issueService: IssuesService) { }

    public ngOnInit() {
        this.isAdmin = this.authService.isAdmin();
        this.user = this.authService.claims();
        this.settingsService.getIssueSettings().subscribe(x => this.settings = x);
        this.issueService.getComments(this.data.issueId).subscribe(x => {
            this.comments = x;
            this.mapMessages();
            this.loaded = true;
        });
    }


    public deleteComment(commentId: number) {

    }

    public addComment(comment: string) {
        this.issueService.addComment({
            comment: comment,
            issueId: this.data.issueId
        }).subscribe(comment => {
            this.messages.push({
                chatType: ChatType.Sender,
                date: comment.date,
                id: -1,
                message: comment.comment,
                username: comment.user.userName
            });
        });

    }

    public close() {
        this.dialogRef.close();
      }

      private mapMessages() {
          this.comments.forEach((m: IIssuesChat) => {
            this.messages.push({
                chatType: m.username === this.user.name ? ChatType.Sender : ChatType.Reciever,
                date: m.date,
                id: m.id,
                message: m.comment,
                username: m.username
            });
          });
        }


}
