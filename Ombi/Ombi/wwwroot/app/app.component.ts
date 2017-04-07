import { Component } from '@angular/core';
import { NotificationService } from './services/notification.service';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './app.component.html'
})
export class AppComponent {

    constructor(public notificationService: NotificationService) { };
}