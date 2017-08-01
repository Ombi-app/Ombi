import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { ServiceHelpers } from './service.helpers';
import { IImages } from '../interfaces/IImages';

@Injectable()
export class ImageService extends ServiceHelpers { 
    constructor(public http : Http) {
        super(http, '/api/v1/Images/');
    }

    getRandomBackground(): Observable<IImages> {
        return this.http.get(`${this.url}background/`, { headers: this.headers }).map(this.extractData);
    }
}