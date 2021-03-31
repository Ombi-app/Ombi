import { BasePage } from "../base.page";

class LocalUserTab {
    get username(): Cypress.Chainable<any> {
        return cy.get('#adminUsername');
    }

    get password(): Cypress.Chainable<any> {
        return cy.get('#adminPassword');
    }

    get next(): Cypress.Chainable<any> {
        return cy.getByData('nextLocalUser');
    }
}

class WelcomeTab {
    get next(): Cypress.Chainable<any> {
        return cy.getByData('nextWelcome');
    }
}

class MediaServerTab {
    get next(): Cypress.Chainable<any> {
        return cy.getByData('nextMediaServer');
    }
}

class OmbiConfigTab {
    get next(): Cypress.Chainable<any> {
        return cy.getByData('nextOmbiConfig');
    }
}


class WizardPage extends BasePage {

    localUserTab: LocalUserTab;
    welcomeTab: WelcomeTab;
    mediaServerTab: MediaServerTab;
    ombiConfigTab: OmbiConfigTab;

    get finishButton(): Cypress.Chainable<any> {
        return cy.get('#finishWizard');
    }

    get matStepsHeader(): Cypress.Chainable<any> {
        return cy.get('mat-step-header');
    }

    constructor() {
        super();
        this.localUserTab = new LocalUserTab();
        this.welcomeTab = new WelcomeTab();
        this.mediaServerTab = new MediaServerTab();
        this.ombiConfigTab = new OmbiConfigTab();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/`, options);
    }
}

export const wizardPage = new WizardPage();
