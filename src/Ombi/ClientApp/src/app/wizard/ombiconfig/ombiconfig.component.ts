import { Component, Input } from "@angular/core";
import { IOmbiConfigModel } from "../models/OmbiConfigModel";
import { WizardService } from "../services/wizard.service";

@Component({
    selector: "wizard-ombi",
    templateUrl: "./ombiconfig.component.html",
    styleUrls: ["../welcome/welcome.component.scss"]
})
export class OmbiConfigComponent {

    @Input() public config: IOmbiConfigModel;
}
