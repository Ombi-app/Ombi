import { Component } from '@angular/core';
import { Router } from '@angular/router';


@Component({
  
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