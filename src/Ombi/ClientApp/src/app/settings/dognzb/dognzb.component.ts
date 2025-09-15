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
    templateUrl: "./dognzb.component.html",
    styleUrls: ["./dognzb.component.scss"]
})
export class DogNzbComponent implements OnInit {

    public form: UntypedFormGroup;

    public profilesRunning: boolean;

    constructor(private readonly settingsService: SettingsService,
                private readonly fb: UntypedFormBuilder,
                private readonly notificationService: NotificationService) { }

    public ngOnInit() {
        this.settingsService.getDogNzbSettings().subscribe(x => {
            this.form = this.fb.group({
                enabled:            [x.enabled],
                apiKey:             [x.apiKey, Validators.required],
                movies:             [x.movies],
                tvShows:            [x.tvShows],
            });
        });
    }

    public onSubmit(form: UntypedFormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = form.value;

        this.settingsService.saveDogNzbSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the DogNzb settings");
            } else {
                this.notificationService.success("There was an error when saving the DogNzb settings");
            }
        });
    }
}
