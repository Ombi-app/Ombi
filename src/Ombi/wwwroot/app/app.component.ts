import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from './services/notification.service';
import { SettingsService } from './services/settings.service';
import { AuthService } from './auth/auth.service';
import { ILocalUser } from './auth/IUserLogin';

import { ICustomizationSettings } from './interfaces/ISettings';

import style from './app.component.css';


@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './app.component.html',
    styles: [style]
})
export class AppComponent implements OnInit {

    constructor(public notificationService: NotificationService, public authService: AuthService, private router: Router, private settingsService: SettingsService
    ) {
    }

    customizationSettings: ICustomizationSettings;
    user: ILocalUser;

    ngOnInit(): void {

        this.user = this.authService.claims();



        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);

        this.router.events.subscribe(() => {
            this.showNav = this.authService.loggedIn();
        });

    }

    hasRole(role: string): boolean {
        return this.user.roles.some(r => r === role)
    }

    logOut() {
        this.authService.logout();
        this.router.navigate(["login"]);
    }

    showNav: boolean;
}