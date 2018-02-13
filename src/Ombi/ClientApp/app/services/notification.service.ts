import { Injectable } from "@angular/core";
import { Message } from "primeng/components/common/api";

@Injectable()
export class NotificationService {
    public messages: Message[] = [];
    public addMessage(message: Message) {
        this.clearMessages();
        this.messages.push(message);
        this.messages = JSON.parse(JSON.stringify(this.messages)); // NOTE: THIS IS A HACK AROUND A BUG https://github.com/primefaces/primeng/issues/2943
    }

    public success(body: string) {
        this.addMessage({ severity: "success", detail: body });
    }

    public info(title: string, body: string) {
        this.addMessage({ severity: "info", detail: body, summary: title });
    }

    public warning(title: string, body: string) {
        this.addMessage({ severity: "warning", detail: body, summary: title });
    }

    public error(body: string) {
        this.addMessage({ severity: "error", detail: body });
    }

    public clearMessages() {
        this.messages = [];
    }
}
