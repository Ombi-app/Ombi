import { Component } from "@angular/core";
import { DiscoverType } from "../carousel-list/carousel-list.component";

@Component({
    templateUrl: "./discover.component.html",
    styleUrls: ["./discover.component.scss"],
})
export class DiscoverComponent {


    public DiscoverType = DiscoverType;

    constructor() { }

}
