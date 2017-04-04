// Config

enum envs {
    local,
    test,
    next,
    live
}

var envVar = "#{Environment}";
var env = envs.local;
if (envs[envVar]) {
    env = envs[envVar];
}

export var config = {
    envs: envs,
    env: env,
    systemJS: {
        bundle: <boolean>{
            [envs.local]: false,
            [envs.test]: true,
            [envs.next]: true,
            [envs.live]: true
        }[env]
    }
}

export default config;