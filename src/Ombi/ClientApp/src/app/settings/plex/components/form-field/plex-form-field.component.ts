import { Component, EventEmitter, Input, Output } from "@angular/core";

@Component({
    selector: "settings-plex-form-field",
    template: `
    <div class="row">
        <div class="col-2 align-self-center">
            {{label}}

            <br>
            <!-- Content Below the label -->
            <ng-content></ng-content>
        </div>
        <div class="md-form-field col-10">
            <mat-form-field appearance="outline" floatLabel=auto *ngIf="type === 'input' || type === 'password'">
                <input matInput placeholder={{placeholder}} [attr.type]="type" id="{{id}}" name="{{id}}" [ngModel]="value" (ngModelChange)="change($event)" value="{{value}}">
            </mat-form-field>

            <mat-slide-toggle *ngIf="type === 'checkbox'" id="{{id}}" [ngModel]="value" (ngModelChange)="change($event)" [checked]="value"></mat-slide-toggle>

            <ng-content select="[below]"></ng-content>
        </div>

        <div class="col-12">
            <ng-content select="[bottom]"></ng-content>
        </div>
    </div>
    `
})
export class PlexFormFieldComponent {

    @Input() public label: string;
    @Input() public value: any;
    @Output() public valueChange = new EventEmitter();
    @Input() public id: string;
    @Input() public placeholder: string;
    @Input() public type: "input" | "checkbox" | "password" = "input"

    public change(newValue: string) {
        this.value = newValue;
        this.valueChange.emit(newValue);
      }
}
