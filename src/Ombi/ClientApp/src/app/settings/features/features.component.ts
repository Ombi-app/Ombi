import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatSlideToggleModule } from "@angular/material/slide-toggle";
import { MatTooltipModule } from "@angular/material/tooltip";
import { TranslateModule } from "@ngx-translate/core";

import { FeaturesFacade } from "../../state/features";
import { IFeatureEnablement } from "../../interfaces";
import { MatSlideToggleChange } from "@angular/material/slide-toggle";
import { firstValueFrom } from "rxjs";

@Component({
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSlideToggleModule,
        MatTooltipModule,
        TranslateModule
    ],
    templateUrl: "./features.component.html",
    styleUrls: ["./features.component.scss"]
})
export class FeaturesComponent implements OnInit {

    public features: IFeatureEnablement[];

    constructor(private readonly featuresFacade: FeaturesFacade) { }

    public async ngOnInit() {
        this.featuresFacade.features$().subscribe(x => {
            this.features = x;
        });

    }

    public updateFeature(change: MatSlideToggleChange, feature: IFeatureEnablement) {
        if (change.checked) {
            firstValueFrom(this.featuresFacade.enable(feature));
        } else {
            firstValueFrom(this.featuresFacade.disable(feature));
        }
    }
}
