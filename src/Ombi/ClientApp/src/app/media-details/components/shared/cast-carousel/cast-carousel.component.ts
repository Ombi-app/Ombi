import { Component, Input } from "@angular/core";

@Component({
    selector: "cast-carousel",
    templateUrl: "./cast-carousel.component.html",
    styleUrls: ["./cast-carousel.component.scss"]
})
export class CastCarouselComponent {
    
    @Input() cast: any[];
}
