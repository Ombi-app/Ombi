import { PlatformLocation } from "@angular/common";
import { Component, OnInit } from "@angular/core";

@Component({
    templateUrl: "./mediaserver.component.html",
    styleUrls: ["./mediaserver.component.scss"],
    selector: "wizard-media-server",
})
export class MediaServerComponent implements OnInit {

    public baseUrl: string;
    constructor(private location: PlatformLocation) { }

    public ngOnInit(): void {
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }
    }
}
