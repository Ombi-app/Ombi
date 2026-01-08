import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";

@Component({
    standalone: true,
    template: "<h2>{{ 'ErrorPages.NotFound' | translate }}</h2>",
    imports: [
        CommonModule,
        TranslateModule
    ]
})
export class PageNotFoundComponent { }
