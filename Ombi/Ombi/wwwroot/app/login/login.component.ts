import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from '../auth/auth.service';
import { StatusService } from '../services/status.service';
import { NotificationService } from '../services/notification.service';

@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './login.component.html',
})
export class LoginComponent {
    constructor(private authService: AuthService, private router: Router, private notify: NotificationService, private status: StatusService) {
        this.status.getWizardStatus().subscribe(x => {
            if (!x.result) {
                this.router.navigate(['Wizard']);
            }
        });
    }


    username: string;
    password: string;


    login(): void {
            this.authService.login({ password: this.password, username: this.username })
                .subscribe(x => {
                    localStorage.setItem("id_token", x.access_token);
                    if (this.authService.loggedIn()) {
                        this.router.navigate(['search']);
                    } else {
                        this.notify.error("Could not log in", "Incorrect username or password");
                    }
                }, err => this.notify.error("Could not log in", "Incorrect username or password"));

        
    }
}