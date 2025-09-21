import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";


@Component({
    standalone: true,
    templateUrl: "./grid-spinner.component.html",
    selector: "grid-spinner",
    styleUrls: ["./grid-spinner.component.scss"],
    imports: [
        CommonModule,
        MatProgressSpinnerModule
    ]
})
export class GridSpinnerComponent{
    @Input() public loading = false;   
}
