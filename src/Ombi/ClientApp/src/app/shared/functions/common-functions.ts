export function getBaseLocation() {
    debugger;
    let paths: string[] = location.pathname.split('/').splice(1, 1);
    let basePath: string = (paths && paths[0]) || 'discover'; // Default: discover
    return '/' + basePath;
}