import { Component, Input } from "@angular/core";

@Component({
    selector: "cast-carousel",
    templateUrl: "./cast-carousel.component.html",
})
export class CastCarouselComponent {
    
    @Input() cast: any[];
}
