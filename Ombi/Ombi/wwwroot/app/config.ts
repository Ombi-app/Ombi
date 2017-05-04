// Config

enum envs {
    local = 0,
    next = 1,
    live = 2
}

var envVar = '{something}';
var env = envs.local;
if (envs[envVar]) {
    env = envs[envVar];
}

export var config = {
    envs: envs,
    env: env
}

export default config;
