import { ErrorHandler, Injectable } from "@angular/core";
import { HttpErrorResponse } from "@angular/common/http";

/**
 * HTTP failures are already handled by the HTTP services and the
 * UnauthorizedInterceptor (which logs the user out and redirects to the login
 * page on a 401). Allowing a rejected HttpErrorResponse to reach the global
 * error handler only produces noisy "uncaught" errors in the browser — most
 * notably on first run, where the discover page briefly loads before the user
 * is redirected to the wizard. We log those here instead of rethrowing them,
 * while delegating any other (genuinely unexpected) error to the default
 * behaviour.
 */
@Injectable()
export class GlobalErrorHandler extends ErrorHandler {
    public override handleError(error: any): void {
        const actualError = error?.rejection ?? error;
        if (actualError instanceof HttpErrorResponse) {
            console.error(actualError);
            return;
        }
        super.handleError(error);
    }
}
