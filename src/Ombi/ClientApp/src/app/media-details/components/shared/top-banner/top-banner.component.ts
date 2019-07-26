import { Component, Inject, Input } from "@angular/core";
import { DomSanitizer, SafeStyle } from "@angular/platform-browser";

@Component({
    selector: "top-banner",
    templateUrl: "./top-banner.component.html",
})
export class TopBannerComponent {
    
    @Input() title: string;
    @Input() releaseDate: Date;
    @Input() tagline: string;
    @Input() available: boolean;
    @Input() background: any;

    
    constructor(private sanitizer:DomSanitizer){}    

    public getBackgroundImage(): SafeStyle {
        return this.sanitizer.bypassSecurityTrustStyle(this.background);
    }
}
