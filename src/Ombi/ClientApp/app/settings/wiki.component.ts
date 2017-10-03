import { Component, Input } from "@angular/core";

@Component({
    selector:"wiki",
    templateUrl: "./wiki.component.html",
})
export class WikiComponent {
    @Input() public url: string;
}
