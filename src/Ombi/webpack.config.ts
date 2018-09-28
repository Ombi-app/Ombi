"use strict";
import * as path from "path";
import { Configuration, DllReferencePlugin } from "webpack";
import * as webpackMerge from "webpack-merge";

import { isAOT, isProd, outputDir, WebpackCommonConfig } from "./webpack.config.common";

module.exports = (env: any) => {
    const prod = isProd(env);
    const aot = isAOT(env);
    if (!prod && aot) { console.warn("Vendor dll bundle will not be used as AOT is enabled"); }
    const bundleConfig: Configuration = webpackMerge(WebpackCommonConfig(env, "main"), {
        entry: {
            app: "./ClientApp/main.ts",
        },
        devtool: prod ? "source-map" : "eval-source-map",
        plugins: prod || aot ? [] : [
            // AOT chunk splitting does not work while this is active https://github.com/angular/angular-cli/issues/4565
            new DllReferencePlugin({
                context: __dirname,
                manifest: require(path.join(__dirname, outputDir, "vendor-manifest.json")),
            }),
        ],
    });

    return bundleConfig;
};
