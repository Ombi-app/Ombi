import { EventEmitter, Injectable, Output } from "@angular/core";

import { RequestType } from "../../interfaces";

@Injectable({
    providedIn: "root"
})
export class AdvancedSearchDialogDataService {

    @Output() public onDataChange = new EventEmitter<any>();
    @Output() public onOptionsChange = new EventEmitter<any>();
    private _data: any;
    private _options: any;
    private _type: RequestType;

    setData(data: any, type: RequestType) {
        this._data = data;
        this._type = type;
        this.onDataChange.emit(this._data);
    }

    setOptions(watchProviders: number[], genres: number[], keywords: number[], releaseYear: number, type: RequestType, position: number) {
        this._options = {
            watchProviders,
            genres,
            keywords,
            releaseYear,
            type,
            position
        };
        this.onOptionsChange.emit(this._options);
    }

    getData(): any {
        return this._data;
    }

    getOptions(): any {
        return this._options;
    }

    getLoaded(): number {
        return this._options.loaded;
    }

    getType(): RequestType {
        return this._type;
    }
}