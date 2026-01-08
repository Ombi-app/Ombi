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
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";

import { NotificationService, SettingsService } from "../../services";

import { SettingsMenuComponent } from "../settingsmenu.component";
import { WikiComponent } from "../wiki.component";
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
    templateUrl: "./vote.component.html",
    styleUrls: ["vote.component.scss"]
})
export class VoteComponent implements OnInit {

    public form: UntypedFormGroup;

    constructor(private settingsService: SettingsService,
                private readonly fb: UntypedFormBuilder,
                private notificationService: NotificationService) {  }

    public ngOnInit() {
        this.settingsService.getVoteSettings().subscribe(x => {
            this.form = this.fb.group({
                enabled:                    [x.enabled],
                movieVoteMax:               [x.movieVoteMax, Validators.min(1)],
                musicVoteMax:               [x.musicVoteMax, Validators.min(1)],
                tvShowVoteMax:              [x.tvShowVoteMax, Validators.min(1)],
            });
        });
    }    

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = form.value;

        this.settingsService.saveVoteSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Vote settings");
            } else {
                this.notificationService.success("There was an error when saving the Vote settings");
            }
        });
    }
}
