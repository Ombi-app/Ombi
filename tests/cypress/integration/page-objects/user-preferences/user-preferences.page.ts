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

class MobileTab {

    get qrCode(): Cypress.Chainable<any> {
        return cy.get('#qrCode');
    }

    get noQrCode(): Cypress.Chainable<any> {
        return cy.get('#noQrCode');
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
        return cy.get('[role="tab"]').eq(0);
    }

    get securityTab(): Cypress.Chainable<any> {
        return cy.get('[role="tab"]').eq(1);
    }

    get preferencesTab(): Cypress.Chainable<any> {
        return cy.get('[role="tab"]').eq(2);
    }

    get mobileTab(): Cypress.Chainable<any> {
        return cy.get('[role="tab"]').eq(3);
    }

    profile = new ProfileTab();
    mobile = new MobileTab();
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
