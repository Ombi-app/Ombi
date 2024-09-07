import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, Subject, throwError } from 'rxjs';
import { catchError, throttleTime } from 'rxjs/operators';

import { AuthService } from './auth.service';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {

    private throttleLogout = new Subject();
    constructor(private authService: AuthService, private router: Router) {
        this.throttleLogout.pipe(throttleTime(5000)).subscribe(url => {
            this.authService.logout();
            this.router.navigate(["login"]);
        });
    }

    public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(
            catchError((response: HttpErrorResponse) => {
                if (response.status === 401) {
                    this.throttleLogout.next(request.url);
                }
                return throwError(response);
            }
            ));
    }
}