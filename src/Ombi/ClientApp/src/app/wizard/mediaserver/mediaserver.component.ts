import { PlatformLocation } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { TranslateModule } from "@ngx-translate/core";

@Component({
    standalone: true,
    templateUrl: "./mediaserver.component.html",
    styleUrls: ["./mediaserver.component.scss"],
    selector: "wizard-media-server",
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        TranslateModule
    ]
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
