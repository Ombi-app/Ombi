import { Component, Input } from "@angular/core";

import { Router } from "@angular/router";

@Component({
    selector:"wiki",
    templateUrl: "./wiki.component.html",
})
export class WikiComponent {
    @Input() public text: string;
    public domain: string = "http://docs.ombi.app"

    get url(): string {
        return this.router.url.toLowerCase();
    }

    constructor(private router: Router) { }
}
