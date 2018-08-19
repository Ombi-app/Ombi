"use strict";

const gulp = require("gulp");
const run = require("gulp-run");
const runSequence = require("run-sequence");
const del = require("del");
const path = require("path");
const fs = require("fs");

const outputDir = "./wwwroot/dist";
global.aot = true;

function getEnvOptions() {
    const options = [];
    if (global.prod) {
        options.push("--env.prod");
    }
    if (global.analyse) {
        options.push("--env.analyse");
    }
    if (global.aot) {
        options.push("--env.aot");
    }
    if (options.length > 0) {
        return " " + options.join(" ");
    } else {
        return "";
    }
}

function webpack(type) {
    // 'webpack' instead of direct path can cause https://github.com/angular/angular-cli/issues/6417
    return run(`node ${path.join('node_modules', 'webpack', 'bin', 'webpack.js')} --config webpack.config${type ? `.${type}` : ""}.ts${getEnvOptions()}`).exec();
}

gulp.task("vendor", () => {
    let build = false;
    const vendorPath = path.join(outputDir, "vendor.js");
    const vendorExists = fs.existsSync(vendorPath);
    if (vendorExists) {
        const vendorStat = fs.statSync(vendorPath);
        const packageStat = fs.statSync("package.json");
        const vendorConfigStat = fs.statSync("webpack.config.vendor.ts");
        const commonConfigStat = fs.statSync("webpack.config.common.ts");
        if (packageStat.mtime > vendorStat.mtime) {
            build = true;
        }
        if (vendorConfigStat.mtime > vendorStat.mtime) {
            build = true;
        }
        if (commonConfigStat.mtime > vendorStat.mtime) {
            build = true;
        }
    } else {
        build = true;
    }
    if (build) {
        return webpack("vendor");
    }
});

gulp.task("vendor_force", () => {
    return webpack("vendor");
})

gulp.task("main", () => {
    return webpack()
});

gulp.task("prod_var", () => {
    global.prod = true;
})

gulp.task("analyse_var", () => {
    global.analyse = true;
})

gulp.task("clean", () => {
    del.sync(outputDir, { force: true });
});

gulp.task("lint", () => run("npm run lint").exec());
gulp.task("lint_fix", () => run("npm run lint -- --fix").exec());
gulp.task("build", callback => runSequence("vendor", "main", callback));
gulp.task("analyse", callback => runSequence("analyse_var", "clean", "build", callback));
gulp.task("full", callback => runSequence("clean", "build", callback));
gulp.task("publish", callback => runSequence("prod_var", "full", callback));