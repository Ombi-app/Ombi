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
})

System.import('bundle').then(() => {
    System.import('/app/main');
})