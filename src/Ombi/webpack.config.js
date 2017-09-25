const path = require('path');
const webpack = require('webpack');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const ExtractTextPlugin = require("extract-text-webpack-plugin");
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

module.exports = function (env) {
    const extractCSS = new ExtractTextPlugin('main.css');
    const prod = env && env.prod;
    console.log(prod ? 'Production' : 'Dev' + ' main build');
    const analyse = env && env.analyse;
    if (analyse) { console.log("Analysing build"); }
    const cssLoader = prod ? 'css-loader?minimize' : 'css-loader';
    const outputDir = './wwwroot/dist';
    const bundleConfig = {
        entry: { 'main': './ClientApp/main.ts' },
        stats: { modules: false },
        context: __dirname,
        resolve: { extensions: ['.ts', '.js'] },
        devtool: prod ? 'source-map' : 'eval-source-map',
        output: {
            filename: '[name].js',
            publicPath: '/dist/',
            path: path.join(__dirname, outputDir)
        },
        module: {
            rules: [
                { test: /\.ts$/, include: /ClientApp/, use: ['awesome-typescript-loader?silent=true', 'angular2-template-loader'] },
                { test: /\.html$/, use: 'html-loader?minimize=false' },
                { test: /\.css$/, use: ['to-string-loader', cssLoader] },
                { test: /\.scss$/, include: /ClientApp(\\|\/)app/, use: ["to-string-loader", cssLoader, "sass-loader"] },
                { test: /\.scss$/, include: /ClientApp(\\|\/)styles/, use: ["style-loader", cssLoader, "sass-loader"] },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
            ]
        },
        plugins: [
            new CheckerPlugin(),
            extractCSS,
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require(path.join(__dirname, outputDir, 'vendor-manifest.json'))
            })
        ].concat(prod ? [
            // Plugins that apply in production builds only
            new UglifyJSPlugin()
        ] : [
            // Plugins that apply in development builds only
        ]).concat(analyse ? [
            new BundleAnalyzerPlugin({
                analyzerMode: 'static',
                reportFilename: 'main.html',
                openAnalyzer: false
            })
        ] : [])
    };

    return bundleConfig;
};
