import { AfterContentChecked, AfterViewInit, Component, EventEmitter, Inject, Input, OnInit, Output } from "@angular/core";


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
    selector: "ombi-chat-box",
    templateUrl: "chat-box.component.html",
    styleUrls: ["chat-box.component.scss"],
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
