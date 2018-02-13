'use strict';

const gulp = require('gulp');
const run = require('gulp-run');
const runSequence = require('run-sequence');
const del = require('del');
const path = require('path');
const fs = require('fs');

const outputDir = './wwwroot/dist';

function getEnvOptions() {
    var options = [];
    if (global.prod) {
        options.push('--env.prod');
    }
    if (global.analyse) {
        options.push('--env.analyse');
    }
    if (options.length > 0) {
        return " " + options.join(" ");
    } else {
        return "";
    }
}


function webpack(vendor) {
    return run(`webpack --config webpack.config${vendor ? '.vendor' : ''}.ts${getEnvOptions()}`).exec();
}

gulp.task('vendor', () => {
    let build = false;
    const vendorPath = path.join(outputDir, "vendor.js");
    const vendorExists = fs.existsSync(vendorPath);
    if (vendorExists) {
        const vendorStat = fs.statSync(vendorPath);
        const packageStat = fs.statSync("package.json");
        const vendorConfigStat = fs.statSync("webpack.config.vendor.ts");
        if (packageStat.mtime > vendorStat.mtime) {
            build = true;
        }
        if (vendorConfigStat.mtime > vendorStat.mtime) {
            build = true;
        }
    } else {
        build = true;
    }
    if (build) {
        return webpack(true);
    }
});


gulp.task('vendor_force', () => {
    return webpack(true);
})

gulp.task('main', () => {
    return webpack()
});

gulp.task('prod_var', () => {
    global.prod = true;
})

gulp.task('analyse_var', () => {
    global.analyse = true;
})

gulp.task('clean', () => {
    del.sync(outputDir, { force: true });
});


gulp.task('lint', () => run("npm run lint").exec());
gulp.task('build', callback => runSequence('vendor', 'main', callback));
gulp.task('analyse', callback => runSequence('analyse_var', 'build'));
gulp.task('full', callback => runSequence('clean', 'build'));
gulp.task('publish', callback => runSequence('prod_var', 'build'));