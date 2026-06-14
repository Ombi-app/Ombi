import { BasePage } from "../../base.page";

class GeneralSettingsPage extends BasePage {

    get baseUrl(): Cypress.Chainable<any> {
        return cy.get('[data-test="baseUrl"]');
    }

    get apiKey(): Cypress.Chainable<any> {
        return cy.get('#ApiKey');
    }

    get refreshApiKey(): Cypress.Chainable<any> {
        return cy.get('[data-test="refreshApiKey"]');
    }

    // The "do not send notifications for auto approve" slide toggle - a harmless,
    // side-effect-free boolean that is safe to flip in a shared environment. The
    // legacy mat-slide-toggle host carries the `mat-checked` class when on, which
    // is the reliable signal of its state.
    get autoApproveToggle(): Cypress.Chainable<any> {
        return cy.get('[data-test="doNotSendNotificationsForAutoApprove"]');
    }

    get submit(): Cypress.Chainable<any> {
        return cy.get('#save');
    }

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/Settings/Ombi`, options);
    }

}

export const generalSettingsPage = new GeneralSettingsPage();
