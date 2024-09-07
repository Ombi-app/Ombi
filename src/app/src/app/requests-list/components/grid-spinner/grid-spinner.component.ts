import { Component, Input } from "@angular/core";


@Component({
    templateUrl: "./grid-spinner.component.html",
    selector: "grid-spinner",
    styleUrls: ["./grid-spinner.component.scss"]
})
export class GridSpinnerComponent{
    @Input() public loading = false;   
}
