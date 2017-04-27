import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from './services/notification.service';
import { SettingsService } from './services/settings.service';
import { AuthService } from './auth/auth.service';

import { ICustomizationSettings } from './interfaces/ISettings';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

    constructor(public notificationService: NotificationService, public authService: AuthService, private router: Router, private settingsService : SettingsService) {
    }
    customizationSettings: ICustomizationSettings;

    ngOnInit(): void {

        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);

        this.router.events.subscribe(() => {
            this.username = localStorage.getItem('currentUser');
            this.showNav = this.authService.loggedIn();
        });


    }

    
    logOut() {
        this.authService.logout();
        this.router.navigate(["login"]);
    }

    username:string;
    showNav :boolean;
}