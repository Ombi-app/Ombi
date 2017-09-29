import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";

import { AuthService } from "../auth/auth.service";
import { ICustomizationSettings } from "../interfaces";
import { NotificationService } from "../services";
import { SettingsService } from "../services";
import { StatusService } from "../services";

import { DomSanitizer } from "@angular/platform-browser";
import { ImageService } from "../services";

@Component({
    templateUrl: "./login.component.html",
    styleUrls: ["./login.component.scss"],
})
export class LoginComponent implements OnInit {

    public form: FormGroup;
    public customizationSettings: ICustomizationSettings;
    public background: any;
    public landingFlag: boolean;

    constructor(private authService: AuthService, private router: Router, private notify: NotificationService, private status: StatusService,
                private fb: FormBuilder, private settingsService: SettingsService, private images: ImageService, private sanitizer: DomSanitizer,
                private route: ActivatedRoute) {
        this.route.params
            .subscribe(params => {
                this.landingFlag = params.landing;
                if (this.landingFlag === false) {
                    this.settingsService.getLandingPage().subscribe(x => {
                        if (x.enabled && !this.landingFlag) {
                            this.router.navigate(["landingpage"]);
                        }
                    });
                }
            });

        this.form = this.fb.group({
            username: ["", [Validators.required]],
            password: ["", [Validators.required]],
            rememberMe: [false],
        });

        this.status.getWizardStatus().subscribe(x => {
            if (!x.result) {
                this.router.navigate(["Wizard"]);
            }
        });
    }

    public ngOnInit() {
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.images.getRandomBackground().subscribe(x => {
            this.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%),url(" + x.url + ")");
        });
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notify.error("Validation", "Please check your entered values");
            return;
        }
        const value = form.value;
        this.authService.login({ password: value.password, username: value.username, rememberMe:value.rememberMe })
            .subscribe(x => {
                localStorage.setItem("id_token", x.access_token);

                if (this.authService.loggedIn()) {
                    this.router.navigate(["search"]);
                } else {
                    this.notify.error("Could not log in", "Incorrect username or password");
                }

            }, err => this.notify.error("Could not log in", "Incorrect username or password"));
    }
}
