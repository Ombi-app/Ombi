import { BasePage } from "../../base.page";

class CustomizationSettingsPage extends BasePage {

    get applicationName(): Cypress.Chainable<any> {
        return cy.get('[data-test="applicationName"]');
    }

    get applicationUrl(): Cypress.Chainable<any> {
        return cy.get('[data-test="applicationUrl"]');
    }

    get hideAvailableToggle(): Cypress.Chainable<any> {
        return cy.get('[data-test="hideAvailableFromDiscover"]');
    }

    get submit(): Cypress.Chainable<any> {
        return cy.get('[data-test="save"]');
    }

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/Settings/Customization`, options);
    }

}

export const customizationSettingsPage = new CustomizationSettingsPage();
