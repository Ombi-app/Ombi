import { Headers, RequestOptions, Response } from '@angular/http';

export class ServiceHelpers {
    public static Headers = new Headers({ 'Content-Type': 'application/json' });

    public static RequestOptions = new RequestOptions({
        headers: ServiceHelpers.Headers
    });

    public static extractData(res: Response) {
        console.log(res);
        return res.json();
    }

}