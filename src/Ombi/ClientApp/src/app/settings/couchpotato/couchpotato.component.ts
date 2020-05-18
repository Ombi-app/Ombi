import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";

import { CouchPotatoService, NotificationService, SettingsService, TesterService } from "../../services";

import { ICouchPotatoProfiles } from "../../interfaces";

@Component({
    templateUrl: "./couchpotato.component.html",
    styleUrls: ["./couchpotato.component.scss"]
})
export class CouchPotatoComponent implements OnInit {

    public form: FormGroup;
    public profiles: ICouchPotatoProfiles;

    public profilesRunning: boolean;

    constructor(private readonly settingsService: SettingsService,
                private readonly fb: FormBuilder,
                private readonly notificationService: NotificationService,
                private readonly couchPotatoService: CouchPotatoService,
                private readonly testerService: TesterService) { }

    public ngOnInit() {
        this.settingsService.getCouchPotatoSettings().subscribe(x => {
            this.form = this.fb.group({
                enabled:            [x.enabled],
                username:           [x.username],
                password:           [x.password],
                apiKey:             [x.apiKey, Validators.required],
                ip:                 [x.ip, Validators.required],
                port:               [x.port, Validators.required],
                ssl:                [x.ssl],
                subDir:             [x.subDir],
                defaultProfileId:   [x.defaultProfileId],
            });

            if(x.defaultProfileId) {
                this.getProfiles(this.form);
            }
        });
    }

    public getProfiles(form: FormGroup) {
        this.profilesRunning = true;
        this.couchPotatoService.getProfiles(form.value).subscribe(x => {
            this.profiles = x;
            this.profilesRunning = false;
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = form.value;

        this.settingsService.saveCouchPotatoSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the CouchPotato settings");
            } else {
                this.notificationService.success("There was an error when saving the CouchPotato settings");
            }
        });
    }

    public test(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        const settings = form.value;
        this.testerService.couchPotatoTest(settings).subscribe(x => {
            if (x === true) {
                this.notificationService.success("Successfully connected to CouchPotato!");
            } else {
                this.notificationService.error("We could not connect to CouchPotato!");
            }
        });
    }

    public requestToken(form: FormGroup) {
        this.couchPotatoService.getApiKey(form.value).subscribe(x => {
            if (x.success === true) {
                (<FormControl>this.form.controls.apiKey).setValue(x.api_key);
                this.notificationService.success("Successfully grabbed the Api Key");
            } else {
                this.notificationService.error("Could not get the Api Key");
            }
        });
    }
}
