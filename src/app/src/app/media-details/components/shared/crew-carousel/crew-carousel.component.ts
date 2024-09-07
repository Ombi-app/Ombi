import { Component, Input } from "@angular/core";

@Component({
    selector: "crew-carousel",
    templateUrl: "./crew-carousel.component.html",
    styleUrls: ["./crew-carousel.component.scss"]
})
export class CrewCarouselComponent {

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

    @Input() crew: any[];
    public responsiveOptions: any[];
}
