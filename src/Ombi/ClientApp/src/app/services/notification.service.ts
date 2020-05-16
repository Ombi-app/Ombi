import { Injectable } from "@angular/core";
import { Message } from "primeng/components/common/api";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material/snack-bar";

@Injectable()
export class NotificationService {
    constructor(private snackbar: MatSnackBar) { }

    private config: MatSnackBarConfig<any> = {
        duration:3000,
        
    }
    public messages: Message[] = [];
    public addMessage(message: Message) {
        this.clearMessages();
        this.messages.push(message);
        this.messages = JSON.parse(JSON.stringify(this.messages)); // NOTE: THIS IS A HACK AROUND A BUG https://github.com/primefaces/primeng/issues/2943
    }

    public success(body: string) {
        this.snackbar.open(body, "OK", this.config);
    }

    public info(title: string, body: string) {
        this.snackbar.open(body, "OK", this.config);
    }

    public warning(title: string, body: string) {
        this.snackbar.open(body, "OK", this.config);
    }

    public error(body: string) {
        this.snackbar.open(body, "OK", this.config);
    }

    public clearMessages() {
        this.messages = [];
    }
}
