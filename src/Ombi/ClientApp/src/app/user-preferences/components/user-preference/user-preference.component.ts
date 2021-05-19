import { Component, Inject, OnInit } from "@angular/core";
import { AuthService } from "../../../auth/auth.service";
import { TranslateService } from "@ngx-translate/core";
import { AvailableLanguages, ILanguage } from "./user-preference.constants";
import { IdentityService, NotificationService, SettingsService, ValidationService } from "../../../services";
import { ICustomizationSettings, IUser, UserType } from "../../../interfaces";
import { Md5 } from "ts-md5";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { APP_BASE_HREF } from "@angular/common";

@Component({
    templateUrl: "./user-preference.component.html",
    styleUrls: ["./user-preference.component.scss"],
})
export class UserPreferenceComponent implements OnInit {

    public username: string;
    public selectedLang: string;
    public availableLanguages = AvailableLanguages;
    public qrCode: string;
    public qrCodeEnabled: boolean;
    public countries: string[];
    public selectedCountry: string;
    public customizationSettings: ICustomizationSettings;
    public UserType = UserType;
    public baseUrl: string;

    public passwordForm: FormGroup;

    private user: IUser;

    constructor(private authService: AuthService,
        private readonly translate: TranslateService,
        private readonly notification: NotificationService,
        private readonly identityService: IdentityService,
        private readonly settingsService: SettingsService,
        private readonly fb: FormBuilder,
        private readonly validationService: ValidationService,
        @Inject(APP_BASE_HREF) public internalBaseUrl: string) { }

    public async ngOnInit() {
        if (this.internalBaseUrl.length > 1) {
            this.baseUrl = this.internalBaseUrl;
        }
        const user = this.authService.claims();
        if (user.name) {
            this.username = user.name;
        }
        this.customizationSettings = await this.settingsService.getCustomization().toPromise();

        this.selectedLang = this.translate.currentLang;

        const accessToken = await this.identityService.getAccessToken().toPromise();
        this.qrCode = `${this.customizationSettings.applicationUrl}|${accessToken}`;

        if(!this.customizationSettings.applicationUrl) {
           this.qrCodeEnabled = false;
        } else {
           this.qrCodeEnabled = true;
        }

        this.user = await this.identityService.getUser().toPromise();
        this.selectedCountry = this.user.streamingCountry;
        this.identityService.getSupportedStreamingCountries().subscribe(x => this.countries = x);
        this.settingsService.getCustomization().subscribe(x => this.customizationSettings = x);

        this.passwordForm = this.fb.group({
            password: [null],
            currentPassword: [null, Validators.required],
            confirmPassword: [null],
            emailAddress: [this.user.emailAddress,  Validators.email]
        })

        this.passwordForm.controls.password.valueChanges.subscribe(x => {
            if (x) {
                this.validationService.enableValidation(this.passwordForm, "confirmPassword");
            }
        });
        this.passwordForm.controls.confirmPassword.valueChanges.subscribe(x => {
            if (x) {
                this.validationService.enableValidation(this.passwordForm, "password");
            }
        });
    }

    public languageSelected() {
        this.identityService.updateLanguage(this.selectedLang).subscribe(x => this.notification.success(this.translate.instant("UserPreferences.Updated")));
        this.translate.use(this.selectedLang);
    }

    public countrySelected() {
        this.identityService.updateStreamingCountry(this.selectedCountry).subscribe(x => this.notification.success(this.translate.instant("UserPreferences.Updated")));
    }

    public getProfileImage(): string {
        let emailHash: string|Int32Array;
        if (this.user.emailAddress) {
          const md5 = new Md5();
          emailHash = md5.appendStr(this.user.emailAddress).end();
        }
        var fallback = this.customizationSettings.logo ? this.customizationSettings.logo : 'https://raw.githubusercontent.com/Ombi-app/Ombi/gh-pages/img/android-chrome-512x512.png';
        return `https://www.gravatar.com/avatar/${emailHash}?d=${fallback}`;
    }

    public updatePassword() {
        if (this.passwordForm.invalid) {
            this.passwordForm.markAsDirty();
            return;
        }

        var values = this.passwordForm.value;

        this.identityService.updateLocalUser({
            password: values.password,
            confirmNewPassword: values.confirmPassword,
            emailAddress: values.emailAddress,
            id: this.user.id,
            currentPassword: values.currentPassword
        }).subscribe(x => {
            if (x.successful) {
                this.notification.success("Updated your information");
                this.user.emailAddress = values.emailAddress;
            } else {
                this.notification.error(x.errors[0]);
            }
        })
    }

    public openMobileApp(event: any) {
        event.preventDefault();

        const url = `ombi://${this.qrCode}`;
        window.location.assign(url);
    }


    private welcomeText: string;
    private setWelcomeText() {
    var d = new Date();
    var hour = d.getHours();

    if (hour >= 0 && hour < 12) {
      this.welcomeText = 'NavigationBar.MorningWelcome';
    }
    if (hour >= 12 && hour < 18) {
      this.welcomeText = 'NavigationBar.AfternoonWelcome';
    }
    if (hour >= 18 && hour < 24) {
      this.welcomeText = 'NavigationBar.EveningWelcome';
    }
  }
}
