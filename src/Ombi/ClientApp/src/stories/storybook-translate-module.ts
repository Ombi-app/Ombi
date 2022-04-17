import { TranslateLoader, TranslateModule, TranslateService } from "@ngx-translate/core";

import { HttpClient } from "@angular/common/http";
import { HttpLoaderFactory } from "../app/app.module";
import { NgModule } from "@angular/core";
import { PlatformLocation } from "@angular/common";

/**
  A utility module adding I18N support for Storybook stories
 **/
  @NgModule({
    imports: [
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient, PlatformLocation],
            },
        }),
    ],
  })
  export class StorybookTranslateModule {
    constructor(translateService: TranslateService) {
      console.log("Configuring the translation service: ", translateService);
      console.log("Translations: ", translateService.translations);
      translateService.setDefaultLang("en-US");
      translateService.use("en-US");
    }
  }