"use strict";
System.config({
    baseURL: '/lib',
    packages: {
        '.': {
            defaultExtension: 'js'
        }
    },
    map: { app: '../app' }
});
System.import('bundle').then(function () {
    System.import('/app/main');
});
//# sourceMappingURL=systemjs.config.js.map