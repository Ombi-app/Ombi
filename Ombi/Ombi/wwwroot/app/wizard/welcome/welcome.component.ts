import { Component } from '@angular/core';
import { Router } from '@angular/router';


@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './welcome.component.html',
})
export class WelcomeComponent {
    constructor(private router: Router) {

    }

    next() {
        this.router.navigate(['Wizard/MediaServer']);
    }
}