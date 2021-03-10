/// <reference types="cypress" />


declare namespace Cypress {
    interface Chainable {

        landingSettings(enabled: boolean): Chainable<any>;
        login(username: string, password: string): Chainable<any>;
        login(): Chainable<any>;
        removeLogin(): Chainable<any>;
        verifyNotification(text: string): Chainable<any>;
        createUser(username: string, password: string, claims: any[]): Chainable<any>;
        generateUniqueId(): Chainable<string>;
        getByData(selector: string, args: any[]): Chainable<any>;
        getByData(selector: string): Chainable<any>;
        getByDataLike(selector: string): Chainable<any>;
        triggerHover(): Chainable<any>;

        requestGenericMovie(): Chainable<any>;
        requestMovie(movieId: number): Chainable<any>;
        requestAllTv(tvId: number): Chainable<any>;
        removeAllMovieRequests(): Chainable<any>;
    }
}
