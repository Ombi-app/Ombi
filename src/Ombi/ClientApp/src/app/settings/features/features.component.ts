import { Component, OnInit } from "@angular/core";

import { FeaturesFacade } from "../../state/features";
import { IFeatureEnablement } from "../../interfaces";
import { MatSlideToggleChange } from "@angular/material/slide-toggle";
import { firstValueFrom } from "rxjs";

@Component({
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
