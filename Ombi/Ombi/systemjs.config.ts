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

    System.import(config.bundle ? 'bundles/full' : 'bundles/lib').then(() => {
        System.import('/app/main');
    })
});