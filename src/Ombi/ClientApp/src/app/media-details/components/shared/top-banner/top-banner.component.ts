import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { DomSanitizer, SafeStyle } from "@angular/platform-browser";

@Component({
        standalone: true,
    selector: "top-banner",
    templateUrl: "./top-banner.component.html",
    styleUrls: ["top-banner.component.scss"],
    imports: [
        CommonModule
    ]
})
export class TopBannerComponent {
    @Input() title: string;
    @Input() releaseDate: Date;
    @Input() tagline: string;
    @Input() available: boolean;
    @Input() background: any;

    get releaseDateFormat(): Date|null {
        if (this.releaseDate && this.releaseDate instanceof Date && this.releaseDate.getFullYear() !== 1) {
            return this.releaseDate;
        }
        return null;
    }

    constructor(private sanitizer:DomSanitizer){ }

    public getBackgroundImage(): SafeStyle {
        return this.sanitizer.bypassSecurityTrustStyle(this.background);
    }
}
