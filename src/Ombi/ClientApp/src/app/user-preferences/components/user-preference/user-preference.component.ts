import { Component, OnInit } from "@angular/core";
import { AuthService } from "../../../auth/auth.service";
import { TranslateService } from "@ngx-translate/core";
import { AvailableLanguages, ILanguage } from "./user-preference.constants";
import { StorageService } from "../../../shared/storage/storage-service";

@Component({
    templateUrl: "./user-preference.component.html",
    styleUrls: ["./user-preference.component.scss"],
})
export class UserPreferenceComponent implements OnInit {
    
    public username: string;
    public selectedLang: string;
    public availableLanguages = AvailableLanguages;

    constructor(private authService: AuthService,
                private readonly translate: TranslateService,
                private storage: StorageService) { }

    public ngOnInit(): void {
        const user = this.authService.claims();
        if(user.name) {
            this.username = user.name;
        }
        const selectedLang = this.storage.get("Language");
        if(selectedLang) {
            this.selectedLang = selectedLang;
        }
    }

    public languageSelected() {
        this.storage.save("Language", this.selectedLang);
        this.translate.use(this.selectedLang);
    }

}