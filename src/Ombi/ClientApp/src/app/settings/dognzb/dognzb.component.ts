import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { NotificationService, SettingsService } from "../../services";

@Component({
    templateUrl: "./dognzb.component.html",
    styleUrls: ["./dognzb.component.scss"]
})
export class DogNzbComponent implements OnInit {

    public form: FormGroup;

    public profilesRunning: boolean;

    constructor(private readonly settingsService: SettingsService,
                private readonly fb: FormBuilder,
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

    public onSubmit(form: FormGroup) {
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
