import { AfterContentChecked, AfterViewInit, Component, EventEmitter, Inject, Input, OnInit, Output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatIconModule } from "@angular/material/icon";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";
import { OmbiDatePipe } from "app/pipes/OmbiDatePipe";

export interface ChatMessages {
    id: number;
    message: string;
    date: Date;
    username: string;
    chatType: ChatType;
  }

  export enum ChatType {
    Sender,
    Reciever
  }

@Component({
    standalone: true,
    selector: "ombi-chat-box",
    templateUrl: "chat-box.component.html",
    styleUrls: ["chat-box.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        MatTooltipModule,
        TranslateModule,
        OmbiDatePipe,
    ]
})
export class ChatBoxComponent implements OnInit {
    @Input() messages: ChatMessages[];
    @Output() onAddMessage: EventEmitter<string> = new EventEmitter<string>();
    @Output() onDeleteMessage: EventEmitter<number> = new EventEmitter<number>();

    public currentMessage: string;
    public userList: string[];
    public ChatType = ChatType;

    public ngOnInit(): void {
        const allUsernames = this.messages.map(x => x.username);
        this.userList = allUsernames.filter((v, i, a) => a.indexOf(v) === i);
    }

    public deleteMessage(id: number) {
        this.onDeleteMessage.emit(id);
    }

    public addMessage() {
        if (this.currentMessage) {
            this.onAddMessage.emit(this.currentMessage);
            this.currentMessage = '';
        }
    }
}
