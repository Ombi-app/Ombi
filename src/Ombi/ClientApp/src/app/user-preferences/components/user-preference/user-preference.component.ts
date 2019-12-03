import { Component, OnInit } from "@angular/core";
import { AuthService } from "../../../auth/auth.service";
import { TranslateService } from "@ngx-translate/core";
import { AvailableLanguages, ILanguage } from "./user-preference.constants";
import { StorageService } from "../../../shared/storage/storage-service";
import { IdentityService, SettingsService } from "../../../services";

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

    constructor(private authService: AuthService,
        private readonly translate: TranslateService,
        private storage: StorageService,
        private readonly identityService: IdentityService,
        private readonly settingsService: SettingsService) { }

    public async ngOnInit() {
        const user = this.authService.claims();
        if (user.name) {
            this.username = user.name;
        }
debugger;
        const customization = await this.settingsService.getCustomization().toPromise();

        const accessToken = await this.identityService.getAccessToken().toPromise();
        this.qrCode = `${customization.applicationUrl}|${accessToken}`;

        if(!customization.applicationUrl) {
            this.qrCodeEnabled = false;
        } else {
            this.qrCodeEnabled = true;
        }

        const selectedLang = this.storage.get("Language");
        if (selectedLang) {
            this.selectedLang = selectedLang;
        }
    }

    public languageSelected() {
        this.storage.save("Language", this.selectedLang);
        this.translate.use(this.selectedLang);
    }

}