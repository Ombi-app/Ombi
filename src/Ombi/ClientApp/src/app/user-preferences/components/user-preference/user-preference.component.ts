import { Component, OnInit } from "@angular/core";
import { AuthService } from "../../../auth/auth.service";
import { TranslateService } from "@ngx-translate/core";
import { AvailableLanguages, ILanguage } from "./user-preference.constants";
import { StorageService } from "../../../shared/storage/storage-service";
import { IdentityService, SettingsService } from "../../../services";
import { IUser } from "../../../interfaces";

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

    private user: IUser;

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
        const customization = await this.settingsService.getCustomization().toPromise();

        const accessToken = await this.identityService.getAccessToken().toPromise();
        this.qrCode = `${customization.applicationUrl}|${accessToken}`;

        if(!customization.applicationUrl) {
           this.qrCodeEnabled = false;
        } else {
           this.qrCodeEnabled = true;
        }

        this.user = await this.identityService.getUser().toPromise();
        if (this.user.language) {
            this.selectedLang = this.user.language;
        }
    }

    public languageSelected() {
        this.identityService.updateLanguage(this.selectedLang).subscribe();
        this.translate.use(this.selectedLang);
    }

}
