import { Component, Inject, Input } from "@angular/core";

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
    
}
