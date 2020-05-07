import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

import { NotificationService, SettingsService } from "../../services";

@Component({
    templateUrl: "./vote.component.html",
    styleUrls: ["vote.component.scss"]
})
export class VoteComponent implements OnInit {

    public form: FormGroup;

    constructor(private settingsService: SettingsService,
                private readonly fb: FormBuilder,
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

    public onSubmit(form: FormGroup) {
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
