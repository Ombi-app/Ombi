"use strict";
System.config({
    baseURL: '/lib',
    packages: {
        '.': {
            defaultExtension: 'js'
        }
    },
    map: {
        app: '../app',
        text: '../app/text-loader'
    },
    meta: {
        '*.css': { loader: 'text' },
        '*.html': { loader: 'text' }
    }
});
System.import('bundle').then(function () {
    System.import('/app/main');
});
//# sourceMappingURL=systemjs.config.js.map