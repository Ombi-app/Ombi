import { BasePage } from "../base.page";

class UserPreferencesPage extends BasePage {

    get languageSelectBox(): Cypress.Chainable<any> {
        return cy.get('#langSelect');
    }

    languageSelectBoxOption(lang: string): Cypress.Chainable<any> {
        return cy.get('#langSelect'+lang);
    }

    get streamingSelectBox(): Cypress.Chainable<any> {
        return cy.get('#streamingSelect');
    }

    streamingSelectBoxOption(country: string): Cypress.Chainable<any> {
        return cy.get('#streamingSelect'+country);
    }

    get qrCode(): Cypress.Chainable<any> {
        return cy.get('#qrCode');
    }

    get noQrCode(): Cypress.Chainable<any> {
        return cy.get('#noQrCode');
    }

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/user-preferences`, options);
    }
}

export const userPreferencesPage = new UserPreferencesPage();
