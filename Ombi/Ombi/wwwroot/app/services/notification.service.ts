import { Injectable } from '@angular/core';
import { Message } from 'primeng/components/common/api';

@Injectable()
export class NotificationService {
    messages: Message[] = [];
    public addMessage(message: Message) {
        this.messages.push(message);
    }

    public success(title: string, body: string) {
        this.addMessage({ severity: 'success', detail: body, summary: title });
    }

    public info(title: string, body: string) {
        this.addMessage({ severity: 'info', detail: body, summary: title });
    }

    public warning(title: string, body: string) {
        this.addMessage({ severity: 'warning', detail: body, summary: title });
    }

    public error(title: string, body: string) {
        this.addMessage({ severity: 'error', detail: body, summary: title });
    }

    public clearMessages() {
        this.messages = [];
    }
}