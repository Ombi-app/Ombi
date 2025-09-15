import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatCardModule } from "@angular/material/card";
import { MatIconModule } from "@angular/material/icon";
import { TranslateModule } from "@ngx-translate/core";
import { IOmbiConfigModel } from "../models/OmbiConfigModel";
import { WizardService } from "../services/wizard.service";

@Component({
    standalone: true,
    selector: "wizard-ombi",
    templateUrl: "./ombiconfig.component.html",
    styleUrls: ["../welcome/welcome.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatCardModule,
        MatIconModule,
        TranslateModule
    ]
})
export class OmbiConfigComponent {

    @Input() public config: IOmbiConfigModel;
}
