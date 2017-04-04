// TypeScript transpiles our app to ES5 but some dependencies are written in ES6 so must polyfill
import 'core-js/es6/string';
import 'core-js/es6/array';
import 'core-js/es6/object';

import 'core-js/es7/reflect';
import 'zone.js/dist/zone';

import { config } from './config';

if (config.env === config.envs.local) {
    Error['stackTraceLimit'] = Infinity;
    require('zone.js/dist/long-stack-trace-zone');
}