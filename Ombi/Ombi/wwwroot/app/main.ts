import './polyfills';

import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app.module';
import { config } from './config';

if (config.env !== config.envs.local) {
    enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule);