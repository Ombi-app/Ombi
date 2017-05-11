export var translate = function (this: any, load: any) {
    return "exports.default = " + JSON.stringify(load.source) + ";";
}  