import { Component, OnInit } from "@angular/core";
import { AuthService } from "../../../auth/auth.service";
import { TranslateService } from "@ngx-translate/core";
import { AvailableLanguages, ILanguage } from "./user-preference.constants";
import { StorageService } from "../../../shared/storage/storage-service";
import { IdentityService, NotificationService, SettingsService } from "../../../services";
import { ICustomizationSettings, IUser } from "../../../interfaces";

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

    private user: IUser;

    constructor(private authService: AuthService,
        private readonly translate: TranslateService,
        private readonly notification: NotificationService,
        private readonly identityService: IdentityService,
        private readonly settingsService: SettingsService) { }

    public async ngOnInit() {
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

    }

    public languageSelected() {
        this.identityService.updateLanguage(this.selectedLang).subscribe(x => this.notification.success(this.translate.instant("UserPreferences.Updated")));
        this.translate.use(this.selectedLang);
    }

    public countrySelected() {
        this.identityService.updateStreamingCountry(this.selectedCountry).subscribe(x => this.notification.success(this.translate.instant("UserPreferences.Updated")));
    }

    public openMobileApp(event: any) {
        event.preventDefault();

        this.identityService.getAccessToken().subscribe(x => {
            const url = `ombi://${this.customizationSettings.applicationUrl}_${x}`;
            window.location.assign(url);
        });
    }
}
