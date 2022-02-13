import { Component, OnInit } from "@angular/core";

import { FeaturesFacade } from "../../state/features";
import { IFeatureEnablement } from "../../interfaces";
import { firstValueFrom } from "rxjs";

@Component({
    templateUrl: "./features.component.html"
})
export class FeaturesComponent implements OnInit {

    public features: IFeatureEnablement[];

    constructor(private readonly featuresFacade: FeaturesFacade) { }

    public async ngOnInit() {
        this.featuresFacade.features$().subscribe(x => this.features = x);
    }

    public enableFeature(feature: IFeatureEnablement) {
        firstValueFrom(this.featuresFacade.update(feature));
    }
}
