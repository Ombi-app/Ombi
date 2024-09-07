import { Component, OnInit } from "@angular/core";
import { IdentityService, SettingsService } from "../../../services";

import { ActivatedRoute } from "@angular/router";
import { AuthService } from "../../../auth/auth.service";

@Component({
    templateUrl: "./unsubscribe-confirm.component.html",
})
export class UnsubscribeConfirmComponent implements OnInit {

    private userId: string;

    constructor(private authService: AuthService,
        private readonly identityService: IdentityService,
        private readonly settingsService: SettingsService,
        private route: ActivatedRoute) {
            this.route.params.subscribe(async (params: any) => {
                if (typeof params.id === 'string' || params.id instanceof String) {
                    this.userId = params.id;
                }
            });
         }

    public async ngOnInit() {
        this.identityService.unsubscribeNewsletter(this.userId).subscribe()
    }
}