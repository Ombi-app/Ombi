import { Component, Input } from "@angular/core";

@Component({
    selector:"wiki",
    templateUrl: "./wiki.component.html",
})
export class WikiComponent {
    @Input() public path: string;
    @Input() public text: string;
    public domain: string = "http://docs.ombi.app/"
}
