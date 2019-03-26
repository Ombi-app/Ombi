export function getBaseLocation() {
    debugger;
    let paths: string[] = location.pathname.split('/').splice(1, 1);
    let basePath: string = (paths && paths[0] ? paths[0] : ""); 
    if(invalidProxies.indexOf(basePath) === -1){
        return '/';
    }
    return '/' + basePath;
}

const invalidProxies: string[] = [
    'discover',
    'requests-list',
    'Settings',
    'issues',
    'usermanagement',
    'recentlyadded',
    'details',
    'vote'
]