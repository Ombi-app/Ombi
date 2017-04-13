import { Component } from '@angular/core';
import { Router } from '@angular/router';


@Component({
    selector: 'ombi',
    moduleId: module.id,
    templateUrl: './mediaserver.component.html',
})
export class MediaServerComponent {
    constructor(private router: Router) {

    }

    plex() {
        this.router.navigate(['Wizard/Plex']);
    }

    emby() {
        this.router.navigate(['Wizard/Emby']);
    }
}