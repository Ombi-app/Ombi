import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";

import { PlatformLocation } from "@angular/common";
import { AuthService } from "../auth/auth.service";
import { IAuthenticationSettings, ICustomizationSettings } from "../interfaces";
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
    public authenticationSettings: IAuthenticationSettings;
    public background: any;
    public landingFlag: boolean;
    public baseUrl: string;
    
    private errorBody: string;
    private errorValidation: string;

    constructor(private authService: AuthService, private router: Router, private notify: NotificationService, private status: StatusService,
                private fb: FormBuilder, private settingsService: SettingsService, private images: ImageService, private sanitizer: DomSanitizer,
                private route: ActivatedRoute, private location: PlatformLocation, private readonly translate: TranslateService) {
        this.route.params
            .subscribe((params: any) => {
                this.landingFlag = params.landing;
                if (!this.landingFlag) {
                    this.settingsService.getLandingPage().subscribe(x => {
                        if (x.enabled && !this.landingFlag) {
                            this.router.navigate(["landingpage"]);
                        }
                    });
                }
            });

        this.form = this.fb.group({
            username: ["", [Validators.required]],
            password: [""],
            rememberMe: [false],
        });

        this.status.getWizardStatus().subscribe(x => {
            if (!x.result) {
                this.router.navigate(["Wizard"]);
            }
        });

        if(authService.loggedIn()) {
            this.router.navigate(["search"]);
        }
    }

    public ngOnInit() {
        this.settingsService.getAuthentication().subscribe(x => this.authenticationSettings = x);
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);
        this.images.getRandomBackground().subscribe(x => {
            this.background = this.sanitizer.bypassSecurityTrustStyle("linear-gradient(-10deg, transparent 20%, rgba(0,0,0,0.7) 20.0%, rgba(0,0,0,0.7) 80.0%, transparent 80%),url(" + x.url + ")");
        });
        const base = this.location.getBaseHrefFromDOM();
        if (base.length > 1) {
            this.baseUrl = base;
        }

        this.translate.get("Login.Errors.IncorrectCredentials").subscribe(x => this.errorBody = x);
        this.translate.get("Common.Errors.Validation").subscribe(x => this.errorValidation = x);
    }

    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notify.error(this.errorValidation);
            return;
        }
        const value = form.value;
        const user = { password: value.password, username: value.username, rememberMe:value.rememberMe };
        this.authService.requiresPassword(user).subscribe(x => {
            if(x && this.authenticationSettings.allowNoPassword) {
                // Looks like this user requires a password
                this.authenticationSettings.allowNoPassword = false;
                return;
            }
            this.authService.login(user)
                .subscribe(x => {
                    localStorage.setItem("id_token", x.access_token);

                    if (this.authService.loggedIn()) {
                        this.router.navigate(["search"]);
                    } else {
                        this.notify.error(this.errorBody);
                    }

                }, err => this.notify.error(this.errorBody));
        });
    }
}
