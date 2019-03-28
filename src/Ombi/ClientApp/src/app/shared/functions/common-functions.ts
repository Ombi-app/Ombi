export function getBaseLocation() {
    debugger;
    let paths: string[] = location.pathname.split('/').splice(1, 1);
    let basePath: string = (paths && paths[0] ? paths[0] : ""); 
    if(invalidProxies.indexOf(basePath.toUpperCase()) === -1){
        return '/' + basePath;
    }
    return '/';
}

const invalidProxies: string[] = [
    'DISCOVER',
    'REQUESTS-LIST',
    'SETTINGS',
    'ISSUES',
    'USERMANAGEMENT',
    'RECENTLYADDED',
    'DETAILS',
    'VOTE',
    'LOGIN',
    'LANDINGPAGE',
    'TOKEN',
    'RESET',
    'CUSTOM',
    'AUTH',
    'WIZARD'    
]