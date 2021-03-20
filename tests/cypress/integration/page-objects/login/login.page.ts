import { BasePage } from "../base.page";

class LoginPage extends BasePage {


    get username(): Cypress.Chainable<any> {
        return cy.get('#username-field');
    }

    get password(): Cypress.Chainable<any> {
        return cy.get('#password-field');
    }

    get ombiSignInButton(): Cypress.Chainable<any> {
        return cy.get('[data-cy=OmbiButton]');
    }

    get plexSignInButton(): Cypress.Chainable<any> {
        return cy.get('[data-cy=oAuthPlexButton]');
    }

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/login`, options);
    }

}

export const loginPage = new LoginPage();
