import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';

import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";

@Component({
    templateUrl: './ombi.component.html',
})
export class OmbiComponent implements OnInit {

    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
        private fb: FormBuilder) { }

    form: FormGroup;

    ngOnInit(): void {
        this.settingsService.getOmbi().subscribe(x => {
            this.form = this.fb.group({
                port: [x.port],
                collectAnalyticData: [x.collectAnalyticData],
                apiKey: [{ value: x.apiKey, disabled: true }],
                externalUrl: [x.externalUrl],
                allowExternalUsersToAuthenticate: [x.allowExternalUsersToAuthenticate]
            });
        });
    }


    refreshApiKey() {
        this.settingsService.resetOmbiApi().subscribe(x => {
            this.form.controls["apiKey"].patchValue(x);
        });
    }

    onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Validation", "Please check your entered values");
            return;
        }

        
        this.settingsService.saveOmbi(form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Ombi settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Ombi settings");
            }
        });
    }
}