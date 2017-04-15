import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from '../../auth/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './plex.component.html',
})
export class PlexComponent {

    constructor(private authService: AuthService,  private notificationService: NotificationService) { }
    

    username: string;
    password: string;

    createUser() {

    }
}