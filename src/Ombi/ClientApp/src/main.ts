// Main
import "jquery";

import "bootstrap/dist/js/bootstrap";

import "./styles/_imports.scss";

import { environment } from "./environments/environment";

import "./polyfills";

import { enableProdMode } from "@angular/core";
import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";
import { AppModule } from "./app/app.module";

import { hmrBootstrap } from "./hmr";
declare var module: any;
// declare var __webpack_require__: any;
// __webpack_require__.p = '/ombi';
// __webpack_require__ = '/ombi';
if (environment.production) {
    enableProdMode();
  }

const bootstrap = () => platformBrowserDynamic().bootstrapModule(AppModule);

if (environment.hmr) {
    if (module["hot"]) {
      hmrBootstrap(module, bootstrap);
    } else {
      console.error("HMR is not enabled for webpack-dev-server!");
      console.log("Are you using the --hmr flag for ng serve?");
    }
  } else {
    bootstrap().catch(err => console.log(err));
  }
