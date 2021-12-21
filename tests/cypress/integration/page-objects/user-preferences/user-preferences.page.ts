import { BasePage } from "../base.page";

class ProfileTab {
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
}

class SecurityTab {
    get currentPassword(): Cypress.Chainable<any> {
        return cy.get('#currentPassword');
    }

    get email(): Cypress.Chainable<any> {
        return cy.get('#email');
    }

    get newPassword(): Cypress.Chainable<any> {
        return cy.get('#newPassword');
    }

    get confirmPassword(): Cypress.Chainable<any> {
        return cy.get('#confirmPassword');
    }

    get submitButton(): Cypress.Chainable<any> {
        return cy.get('#submitSecurity');
    }
}

class UserPreferencesPage extends BasePage {


    get username(): Cypress.Chainable<any> {
        return cy.get('#usernameTitle');
    }
    get email(): Cypress.Chainable<any> {
        return cy.get('#emailTitle');
    }

    get profileTab(): Cypress.Chainable<any> {
        return cy.get('#mat-tab-label-0-0');
    }

    get securityTab(): Cypress.Chainable<any> {
        cy.waitUntil(() => {
            return cy.get('#mat-tab-label-0-1').should('be.visible');
        });
        return cy.get('#mat-tab-label-0-1');
    }

    profile = new ProfileTab();
    security = new SecurityTab();

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
