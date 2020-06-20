import { Component, Input } from "@angular/core";

@Component({
    selector: "cast-carousel",
    templateUrl: "./cast-carousel.component.html",
    styleUrls: ["./cast-carousel.component.scss"]
})
export class CastCarouselComponent {

    constructor() {
        this.responsiveOptions = [
            {
                breakpoint: '1024px',
                numVisible: 5,
                numScroll: 5
            },
            {
                breakpoint: '768px',
                numVisible: 3,
                numScroll: 3
            },
            {
                breakpoint: '560px',
                numVisible: 1,
                numScroll: 1
            }
        ];
    }

    @Input() cast: any[];
    public responsiveOptions: any[];
}
