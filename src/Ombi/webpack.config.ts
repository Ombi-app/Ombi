import { CheckerPlugin } from "awesome-typescript-loader";
import * as path from "path";
import * as UglifyJSPlugin from "uglifyjs-webpack-plugin";
import { BundleAnalyzerPlugin } from "webpack-bundle-analyzer";

import * as webpack from "webpack";

module.exports = (env: any) => {
    const prod = env && env.prod as boolean;
    console.log(prod ? "Production" : "Dev" + " main build");
    const analyse = env && env.analyse as boolean;
    if (analyse) { console.log("Analysing build"); }
    const cssLoader = prod ? "css-loader?-url&minimize" : "css-loader?-url";
    const outputDir = "./wwwroot/dist";
    const bundleConfig: webpack.Configuration = {
        entry: { main: "./ClientApp/main.ts" },
        stats: { modules: false },
        context: __dirname,
        resolve: { extensions: [".ts", ".js"] },
        devtool: prod ? "source-map" : "eval-source-map",
        output: {
            filename: "[name].js",
            publicPath: "/dist/",
            path: path.join(__dirname, outputDir),
        },
        module: {
            rules: [
                { test: /\.ts$/, include: /ClientApp/, use: ["awesome-typescript-loader?silent=true", "angular2-template-loader"] },
                { test: /\.html$/, use: "html-loader?minimize=false" },
                { test: /\.css$/, use: ["to-string-loader", cssLoader] },
                { test: /\.scss$/, include: /ClientApp(\\|\/)app/, use: ["to-string-loader", cssLoader, "sass-loader"] },
                { test: /\.scss$/, include: /ClientApp(\\|\/)styles/, use: ["style-loader", cssLoader, "sass-loader"] },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: "url-loader?limit=25000" },
            ],
        },
        plugins: [
            new CheckerPlugin(),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require(path.join(__dirname, outputDir, "vendor-manifest.json")),
            }),
        ].concat(prod ? [
            // Plugins that apply in production builds only
            new UglifyJSPlugin({ sourceMap: true }),
        ] : [
            // Plugins that apply in development builds only
        ]).concat(analyse ? [
            new BundleAnalyzerPlugin({
                analyzerMode: "static",
                reportFilename: "main.html",
                openAnalyzer: false,
            }),
        ] : []),
    };

    return bundleConfig;
};
