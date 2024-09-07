import { OmbiCommonModules } from "../modules";
import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";

@Component({
    standalone: true,
    selector: 'ombi-button',
    imports: [...OmbiCommonModules, MatButtonModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <button [id]="id" [type]="type" [class]="class" [data-toggle]="dataToggle" mat-raised-button [data-target]="dataTarget">{{text}}</button>
    `
  })
  export class ButtonComponent {

    @Input() public text: string;

    @Input() public id: string;
    @Input() public type: string = "primary";
    @Input() public class: string;
    @Input('data-toggle') public dataToggle: string;
    @Input('data-target') public dataTarget: string;

  }