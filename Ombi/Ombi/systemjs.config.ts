System.config({
    baseURL: '/lib',
    packages: {
        '.': {
            defaultExtension: 'js'
        }
    },
    map: { app: '../app' }
})

System.import('bundle').then(() => {
    System.import('/app/main');
})