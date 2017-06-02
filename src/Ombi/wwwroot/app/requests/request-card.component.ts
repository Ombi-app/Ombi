import { Component, Input } from '@angular/core';

import { IMediaBase } from '../interfaces/IRequestModel';

@Component({
    selector: 'request-card',
    moduleId: module.id,
    templateUrl: './request-card.component.html'
})
export class RequestCardComponent {
    @Input() request: IMediaBase;
}
