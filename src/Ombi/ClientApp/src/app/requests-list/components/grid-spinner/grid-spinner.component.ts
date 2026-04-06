import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";


@Component({
    standalone: true,
    templateUrl: "./grid-spinner.component.html",
    selector: "grid-spinner",
    styleUrls: ["./grid-spinner.component.scss"],
    imports: [
        CommonModule,
        TranslateModule
    ]
})
export class GridSpinnerComponent{
    @Input() public loading = false;
}
