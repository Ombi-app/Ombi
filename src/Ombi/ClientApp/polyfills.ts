import 'core-js/es6/string';
import 'core-js/es6/array';
import 'core-js/es6/object';

import 'core-js/es7/reflect';
import 'zone.js/dist/zone';

if (module['hot']) {
    Error['stackTraceLimit'] = Infinity;
    require('zone.js/dist/long-stack-trace-zone');
}