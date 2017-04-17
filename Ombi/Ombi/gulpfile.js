/// <binding BeforeBuild='build' ProjectOpened='watch' />
'use strict';
var gulp = require('gulp');
var sass = require('gulp-sass');
var changed = require('gulp-changed');
var rename = require('gulp-rename');
var uglify = require('gulp-uglify');
var sourcemaps = require('gulp-sourcemaps');
var path = require('path');
var del = require('del');
var merge = require('merge-stream');
var gulpif = require('gulp-if');
var runSequence = require('run-sequence');
var cleancss = require('gulp-clean-css');
var filter = require('gulp-filter');
var systemJSBuilder = require('systemjs-builder');
var run = require('gulp-run');
var fs = require('fs');

var wwwroot = './wwwroot';

var paths = {
    wwwroot: wwwroot,
    npm: { // These will be resolved automatically and copied to output directory as its name, only works for pre-bundled modules e.g. angular
        src: [
            '@angular/animations',
            '@angular/animations/browser',
            '@angular/core',
            '@angular/common',
            '@angular/compiler',
            '@angular/platform-browser',
            '@angular/platform-browser-dynamic',
            '@angular/http',
            '@angular/router',
            '@angular/forms',
            '@angular/platform-browser/animations',
            'ngx-infinite-scroll'
        ],
        dest: './lib'
    },
    lib: { // These are simple single-file dependencies with optional rename, for more files or folders use modules
        src: [
            {
                file: './node_modules/systemjs/dist/system.src.js',
                rename: 'system'
            },
            {
                file: './node_modules/systemjs/dist/system-polyfills.src.js',
                rename: 'system-polyfills'
            },
            {
                file: './node_modules/jquery/dist/jquery.min.js',
                rename: 'jquery'
            },
            './bower_components/PACE/pace.js',
            './node_modules/bootstrap/dist/js/bootstrap.js',
            './node_modules/tether/dist/js/tether.js',
            './node_modules/angular2-jwt/angular2-jwt.js',
            './systemjs.config.js',
        ],
        dest: './lib/'
    },
    libcss: [ // Normal css files to be copied
        {
            src: [
                './bower_components/PACE/themes/purple/pace-theme-minimal.css',
                './bower_components/font-awesome/css/font-awesome.css',
                './node_modules/primeng/resources/primeng.css',
                './node_modules/tether/dist/css/tether.css'
            ],
            dest: './css/lib/'
        },
        {
            src: './Styles/**/*.css',
            dest: './css',
            filter: '**/*.css'
        }
    ],
    libfonts: [ // Library fonts
        {
            src: [
                './bower_components/font-awesome/fonts/*'
            ],
            dest: './fonts/lib/'
        },
        {
            src: [
                './node_modules/primeng/resources/themes/omega/fonts/*'
            ],
            dest: './css/fonts/'
        }
    ],
    libimages: [ // Library images
        {
            src: [
                './node_modules/primeng/resources/themes/omega/images/*'
            ],
            dest: './images/'
        }
    ],
    modules: [ // This is for modules with multiple files that require each other, used when npm can't be used
        {
            name: 'zone.js',
            src: ['./node_modules/zone.js/**/*.js'],
            dest: './lib/zone.js/'
        },
        {
            name: 'rxjs',
            src: ['./node_modules/rxjs/**/*.js', '!./node_modules/rxjs/src/**/*.js'],
            dest: './lib/rxjs/'
        },
        {
            name: 'core-js',
            src: ['./node_modules/core-js/**/*.js'],
            dest: './lib/core-js/'
        },
        {
            name: 'primeng',
            src: './node_modules/primeng/**/*.js',
            dest: './lib/primeng/'
        }
    ],
    sass: { // Simple sass->css compilation
        src: ['./Styles/**/*.scss', '!./Styles/primeng/**'],
        dest: './css/',
        filter: '**/*.css'
    },
    bundle: { // This is the config for the bundler, you shouldn't need to change this
        root: './',
        dest: './lib/bundle.js',
        bundle: 'app/main.js',
        app: 'app/**/*',
        config: {
            baseURL: wwwroot,
            packages: {
                '.': {
                    defaultExtension: 'js'
                }
            },
            paths: {
                '*': 'lib/*',
                'app/*': 'app/*'
            }
        }
    }
}

gulp.task('npm', function () {
    var streams = []
    for (let module of paths.npm.src) {
        let file = require.resolve(module);
        streams.push(
            gulp.src(file)
                .pipe(gulpif(global.full, sourcemaps.init()))
                .pipe(gulpif(global.full, uglify({ source_map: true })))
                .pipe(rename((path => { path.basename = module })))
                .pipe(gulpif(global.full, sourcemaps.write('../maps')))
                .pipe(gulp.dest(path.join(paths.wwwroot, paths.npm.dest)))
        );
    }
    return merge(streams);
})

gulp.task('lib', function () {
    var streams = []
    for (let module of paths.lib.src) {
        streams.push(
            gulp.src(typeof module === "string" ? module : module.file)
                .pipe(gulpif(global.full, sourcemaps.init()))
                .pipe(gulpif(global.full, uglify({ source_map: true })))
                .pipe(rename(function (path) {
                    if (typeof module !== "string" && module.rename) {
                        path.basename = module.rename;
                    }
                }))
                .pipe(gulpif(global.full, sourcemaps.write('maps')))
                .pipe(gulp.dest(path.join(paths.wwwroot, paths.lib.dest)))
        );
    }
    return merge(streams);
})

gulp.task('libcss', function () {
    var streams = []
    for (let module of paths.libcss) {
        var f = filter("**/*.css", { restore: true });
        streams.push(
            gulp.src(module.src)
                .pipe(f)
                .pipe(gulpif(global.full, sourcemaps.init()))
                .pipe(gulpif(global.full, cleancss()))
                .pipe(gulpif(global.full, sourcemaps.write(`${module.name ? '.' : ''}./maps/${module.name ? module.name : ''}`)))
                .pipe(f.restore)
                .pipe(gulp.dest(path.join(paths.wwwroot, module.dest)))
        );
    }
    return merge(streams);
})


gulp.task('libfonts', function () {
    var streams = []
    for (let module of paths.libfonts) {
        streams.push(
            gulp.src(module.src)
                .pipe(gulp.dest(path.join(paths.wwwroot, module.dest)))
        );
    }
    return merge(streams);
})


gulp.task('libimages', function () {
    var streams = []
    for (let module of paths.libimages) {
        streams.push(
            gulp.src(module.src)
            .pipe(gulp.dest(path.join(paths.wwwroot, module.dest)))
        );
    }
    return merge(streams);
})

gulp.task('modules', function () {
    var streams = []
    for (let module of paths.modules) {
        streams.push(
            gulp.src(module.src)
                .pipe(gulpif(global.full, sourcemaps.init()))
                .pipe(gulpif(global.full, uglify({ source_map: true })))
                .pipe(gulpif(global.full, sourcemaps.write(`${module.name ? '.' : ''}./maps/${module.name ? module.name : ''}`)))
                .pipe(gulp.dest(path.join(paths.wwwroot, module.dest)))
        );
    }
    return merge(streams);
})

gulp.task('sass', function () {
    return gulp.src(paths.sass.src)
        .pipe(changed(paths.sass.dest))
        .pipe(gulpif(global.full, sourcemaps.init()))
        .pipe(sass({ outputStyle: global.full ? 'compressed' : 'nested' }).on('error', sass.logError))
        .pipe(gulpif(global.full, sourcemaps.write('maps')))
        .pipe(gulp.dest(path.join(paths.wwwroot, paths.sass.dest)))
});


gulp.task('bundle', ['typescript_firstrun'], function () {
    var builder = new systemJSBuilder(paths.bundle.root);
    builder.config(paths.bundle.config);

    del.sync(path.join(paths.wwwroot, paths.bundle.dest), { force: true });
    return builder.bundle(paths.bundle.bundle + (global.full ? '' : ' - [' + paths.bundle.app + ']'), path.join(paths.wwwroot, paths.bundle.dest), {
        sourceMaps: global.full
    })
})

gulp.task('clean', function () {
    return del([
        paths.sass.dest + paths.sass.filter,
        paths.lib.dest,
        paths.bundle.dest,
        ...paths.modules.map(m => m.dest),
        ...paths.libcss.map(m => m.dest + (m.filter ? m.filter : '')),
        ...paths.libfonts.map(m => m.dest)
    ].map(x => path.join(paths.wwwroot, x)), { force: true });
})

// Runs the TypeScript compiler
function runTSC() {
    return run('tsc').exec();
}

// Allows app to bundle libs on first run by compiling the app first, only compiles if entry point doesn't exist
gulp.task('typescript_firstrun', function () {
    var bundle = path.join(paths.wwwroot, paths.bundle.bundle);
    var exists = fs.existsSync(bundle);
    if (!exists) {
        console.log(`'${bundle}' doesn't exist - compiling TypeScript`);
        return runTSC();
    }
})

gulp.task('typescript', function () {
    return runTSC();
});

uglify().on('error',
    function(err) {
        gutil.log(gutil.colors.red('[Error]'), err.toString());
        this.emit('end');
    });

gulp.task('fullvar', () => { global.full = true });
gulp.task('copy', ['lib', 'libcss', 'libfonts', 'libimages', 'npm', 'modules']);
gulp.task('compile', callback => runSequence('copy', 'sass', callback));
gulp.task('build', callback => runSequence('compile', 'bundle', callback));
gulp.task('full', callback => runSequence('clean', 'compile', callback));

// Use this in a build server environment to compile and bundle everything
gulp.task('publish', callback => runSequence('fullvar', 'full', 'typescript', 'bundle', callback));

// Auto compiles sass files on change, note that this doesn't seem to pick up new files at the moment
gulp.task('watch', function () {
    gulp.watch(paths.sass.src, ['sass']);
    gulp.watch('./Styles/**/*.css', ['libcss']); // legacy css
});
