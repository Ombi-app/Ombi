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
    @Input() public viewMode: "cards" | "compact" = "cards";
    @Input() public columns = 6;

    public readonly cardItems = [1, 2, 3, 4, 5, 6];
    public readonly rowItems = [1, 2, 3, 4, 5, 6, 7, 8];

    public get columnItems(): number[] {
        return Array.from({ length: Math.max(1, this.columns) }, (_, i) => i);
    }
}
