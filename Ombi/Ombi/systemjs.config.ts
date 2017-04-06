System.import('/app/config.js').then((module: any) => {
    var config = module.config.systemJS;
    System.config({
        baseURL: '/lib',
        packages: {
            '.': {
                defaultExtension: 'js'
            }
        },
        map: { app: '../app' }
    })

    if (config.bundle) {
        System.import('bundle').then(() => {
            System.import('/app/main');
        })
    } else {
        System.import('/app/main')
    }
});