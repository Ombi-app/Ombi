"use strict";
import { AngularCompilerPlugin } from "@ngtools/webpack";
import * as MiniCssExtractPlugin from "mini-css-extract-plugin";
import * as path from "path";
import { Configuration, ContextReplacementPlugin, ProvidePlugin } from "webpack";
import { BundleAnalyzerPlugin } from "webpack-bundle-analyzer";

export const outputDir = "./wwwroot/dist";

export function isProd(env: any) {
    return env && env.prod as boolean;
}

export function isAOT(env: any) {
    return env && env.aot as boolean;
}

export const WebpackCommonConfig = (env: any, type: string) => {
    const prod = isProd(env);
    const aot = isAOT(env);
    const vendor = type === "vendor";
    console.log(`${prod ? "Production" : "Dev"} ${type} build`);
    console.log(`Output directory: ${outputDir}`);
    console.log(`${aot ? "Using" : "Not using"} AOT compiler`);
    const analyse = env && env.analyse as boolean;
    if (analyse) { console.log("Analysing build"); }
    const cssLoader = prod ? "css-loader?minimize" : "css-loader";
    const bundleConfig: Configuration = {
        mode: prod ? "production" : "development",
        resolve: {
            extensions: [".ts", ".js"],
            alias: {
                pace: "pace-progress",
            },
        },
        output: {
            path: path.resolve(outputDir),
            filename: "[name].js",
            chunkFilename: "[id].[hash].chunk.js",
            publicPath: "/dist/",
        },
        module: {
            rules: [
                { test: /\.ts$/, loader: aot ? "@ngtools/webpack" : ["awesome-typescript-loader?silent=true", "angular2-template-loader", "angular-router-loader"] },
                { test: /\.html$/, use: "html-loader?minimize=false" },
                { test: /\.css$/, use: [MiniCssExtractPlugin.loader, cssLoader] },
                { test: /\.scss$/, exclude: /ClientApp/, use: [MiniCssExtractPlugin.loader, cssLoader, "sass-loader"] },
                { test: /\.scss$/, include: /ClientApp(\\|\/)app/, use: ["to-string-loader", cssLoader, "sass-loader"] },
                { test: /\.scss$/, include: /ClientApp(\\|\/)styles/, use: ["style-loader", cssLoader, "sass-loader"] },
                { test: /\.(png|woff|woff2|eot|ttf|svg|gif)(\?|$)/, use: "url-loader?limit=100000" },
                { test: /[\/\\]@angular[\/\\].+\.js$/, parser: { system: true } }, // ignore System.import warnings https://github.com/angular/angular/issues/21560
            ],
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: "[name].css",
            }),
            new ProvidePlugin({ $: "jquery", jQuery: "jquery", Hammer: "hammerjs/hammer" }), // Global identifiers
        ].concat(aot && !vendor ? [
            new AngularCompilerPlugin({
                mainPath: "./ClientApp/main.ts",
                tsConfigPath: "./tsconfig.json",
                skipCodeGeneration: false,
                compilerOptions: {
                    noEmit: false,
                },
            }),
        ] : [
            // AOT chunk splitting does not work while this is active but doesn't seem to be needed under AOT anyway https://github.com/angular/angular-cli/issues/4431
            new ContextReplacementPlugin(/angular(\\|\/)core(\\|\/)/, path.join(__dirname, "./ClientApp")), // Workaround for https://github.com/angular/angular/issues/14898
        ]).concat(analyse ? [
            new BundleAnalyzerPlugin({
                analyzerMode: "static",
                reportFilename: `${type}.html`,
                openAnalyzer: false,
            }),
        ] : []),
        node: {
            fs: "empty",
        },
    };

    return bundleConfig;
};
