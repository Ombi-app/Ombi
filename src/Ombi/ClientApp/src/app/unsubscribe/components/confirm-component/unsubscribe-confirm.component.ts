import { Component, OnInit } from "@angular/core";
import { IdentityService, SettingsService } from "../../../services";
import { CommonModule } from "@angular/common";
import { MatCardModule } from "@angular/material/card";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { TranslateModule } from "@ngx-translate/core";

import { ActivatedRoute, RouterModule } from "@angular/router";
import { AuthService } from "../../../auth/auth.service";

@Component({
    standalone: true,
    templateUrl: "./unsubscribe-confirm.component.html",
    imports: [
        CommonModule,
        RouterModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        TranslateModule
    ]
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