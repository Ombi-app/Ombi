"use strict";
import * as path from "path";
import * as webpack from "webpack";
import * as webpackMerge from "webpack-merge";
import { isProd, outputDir, WebpackCommonConfig } from "./webpack.config.common";

module.exports = (env: any) => {
    const prod = isProd(env);
    const bundleConfig = webpackMerge(WebpackCommonConfig(env, "vendor"), {
        output: {
            library: "[name]_[hash]",
        },
        entry: {
            vendor: (<string[]>[ // add any vendor styles here e.g. bootstrap/dist/css/bootstrap.min.css
                "pace-progress/themes/orange/pace-theme-flash.css",
                "primeng/resources/primeng.min.css",
                "@angular/material/prebuilt-themes/deeppurple-amber.css",
                "font-awesome/scss/font-awesome.scss",
                "bootswatch/superhero/bootstrap.min.css",
            ]).concat(prod ? [] : [ // used to speed up dev launch time
                "@angular/animations",
                "@angular/common",
                "@angular/common/http",
                "@angular/compiler",
                "@angular/core",
                "@angular/forms",
                "@angular/http",
                "@angular/platform-browser",
                "@angular/platform-browser/animations",
                "@angular/platform-browser-dynamic",
                "@angular/router",
                "@angular/material",
                "@angular/cdk",
                "pace-progress",
                "primeng/primeng",
                "jquery",
                "zone.js",
                "reflect-metadata",
                "core-js",
                "rxjs",
                "css-loader/lib/css-base",
                "core-js/es6/string",
                "core-js/es6/array",
                "core-js/es6/object",
                "core-js/es7/reflect",
                "hammerjs",
                "event-source-polyfill",
                "bootstrap/dist/js/bootstrap",
                "ngx-clipboard",
                "@auth0/angular-jwt",
                "ng2-cookies",
                "@ngx-translate/core",
                "@ngx-translate/http-loader",
                "ngx-order-pipe",
                "@yellowspot/ng-truncate",
                "ngx-editor",
                "ngx-bootstrap",
            ]),
        },
        plugins: prod ? [] : [
            new webpack.DllPlugin({
                path: path.join(__dirname, outputDir, "[name]-manifest.json"),
                name: "[name]_[hash]",
            }),
        ],
    });
    return bundleConfig;
};
