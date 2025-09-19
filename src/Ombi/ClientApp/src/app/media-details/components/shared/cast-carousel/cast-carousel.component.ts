import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { CarouselModule } from "primeng/carousel";
import { MatCardModule } from "@angular/material/card";
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
        MatCardModule,
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
