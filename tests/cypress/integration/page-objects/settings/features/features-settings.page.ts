import { BasePage } from "../../base.page";

class FeaturesSettingsPage extends BasePage {

    // Each feature toggle is rendered with id="enable{featureName}". The legacy
    // mat-slide-toggle host carries the `mat-checked` class when the feature is
    // enabled, which is the reliable signal of its state.
    featureToggle(name: string): Cypress.Chainable<any> {
        return cy.get(`#enable${name}`);
    }

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/Settings/Features`, options);
    }

}

export const featuresSettingsPage = new FeaturesSettingsPage();
