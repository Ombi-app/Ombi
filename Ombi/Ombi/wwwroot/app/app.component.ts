import { Component } from '@angular/core';
import { NotificationService } from './services/notification.service';
import { AuthService } from './auth/auth.service';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './app.component.html'
})
export class AppComponent {

    constructor(public notificationService: NotificationService, public authService : AuthService) { };
}