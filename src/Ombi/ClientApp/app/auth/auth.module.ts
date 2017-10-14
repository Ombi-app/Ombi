import { NgModule } from "@angular/core";
import { Http, RequestOptions } from "@angular/http";
import { RouterModule, Routes } from "@angular/router";
import { AuthConfig, AuthHttp } from "angular2-jwt";
import { CookieService } from "ng2-cookies";
import { CookieComponent } from "./cookie.component";

export function authHttpServiceFactory(http: Http, options: RequestOptions) {
  return new AuthHttp(new AuthConfig({
    tokenName: "id_token",
    tokenGetter: (() => localStorage.getItem("id_token")!),
    globalHeaders: [{ "Content-Type": "application/json" }],
  }), http, options);
}
const routes: Routes = [
  { path: "auth/cookie", component: CookieComponent  },
];

@NgModule({
  imports : [
    RouterModule.forChild(routes),
  ],
  declarations:[
    CookieComponent,
  ],
  providers: [
    {
      provide: AuthHttp,
      useFactory: authHttpServiceFactory,
      deps: [Http, RequestOptions],
    },
    CookieService,
  ],
})
export class AuthModule { }
