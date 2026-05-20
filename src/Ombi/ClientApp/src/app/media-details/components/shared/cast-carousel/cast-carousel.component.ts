import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { CarouselModule } from "primeng/carousel";
import { TranslateModule } from "@ngx-translate/core";
import { RouterModule } from "@angular/router";
import { ImageComponent } from "../../../../components";
import { FormsModule } from "@angular/forms";

@Component({
        standalone: true,
    selector: "cast-carousel",
    templateUrl: "./cast-carousel.component.html",
    styleUrls: ["./cast-carousel.component.scss"],
    imports: [
        CommonModule,
        CarouselModule,
        TranslateModule,
        RouterModule,
        ImageComponent,
        FormsModule,
    ]
})
export class CastCarouselComponent {

    constructor() {
        this.responsiveOptions = [
            {
                breakpoint: '1200px',
                numVisible: 5,
                numScroll: 5
            },
            {
                breakpoint: '1024px',
                numVisible: 4,
                numScroll: 4
            },
            {
                breakpoint: '768px',
                numVisible: 3,
                numScroll: 3
            },
            {
                breakpoint: '560px',
                numVisible: 2,
                numScroll: 2
            }
        ];
    }

    @Input() cast: any[];
    public responsiveOptions: any[];
}
