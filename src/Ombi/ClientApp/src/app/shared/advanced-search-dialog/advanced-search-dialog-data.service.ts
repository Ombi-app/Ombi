import { EventEmitter, Injectable, Output } from "@angular/core";

import { RequestType } from "../../interfaces";

@Injectable({
    providedIn: "root"
})
export class AdvancedSearchDialogDataService {

    @Output() public onDataChange = new EventEmitter<any>();
    private _data: any;
    private _type: RequestType;

    setData(data: any, type: RequestType) {
        this._data = data;
        this._type = type;
        this.onDataChange.emit(this._data);
    }

    getData(): any {
        return this._data;
    }

    getType(): RequestType {
        return this._type;
    }
}