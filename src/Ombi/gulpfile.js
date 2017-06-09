/// <binding BeforeBuild='build' />
'use strict';

const gulp = require('gulp');
const run = require('gulp-run');
const runSequence = require('run-sequence');
const del = require('del');
const path = require('path');

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

gulp.task('vendor', function () {
    return run('webpack --config webpack.config.vendor.js' + getEnvOptions()).exec();
});

gulp.task('main', function () {
    return run('webpack --config webpack.config.js' + getEnvOptions()).exec();
});

gulp.task('test_compile', function () {
    return run('webpack boot-tests=./ClientApp/test/boot-tests.ts' + getEnvOptions()).exec();
});

gulp.task('test_run', function () {
    return run('karma start ClientApp/test/karma.conf.js').exec();
});

gulp.task('prod_var', function () {
    global.prod = true;
})

gulp.task('analyse_var', function () {
    global.analyse = true;
})

gulp.task('clean', function() {
  del.sync(outputDir, { force: true });
});

gulp.task('test', callback => runSequence('test_compile', 'test_run'));
gulp.task('build', callback => runSequence('vendor', 'main', callback));
gulp.task('analyse', callback => runSequence('analyse_var', 'build'));
gulp.task('full', callback => runSequence('clean', 'build'));
gulp.task('publish', callback => runSequence('prod_var', 'build'));