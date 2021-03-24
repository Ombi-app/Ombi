import { BasePage } from "../../base.page";
import { AdminRequestDialog } from "../../shared/AdminRequestDialog";

class MovieInformationPanel {

    get denyReason(): Cypress.Chainable<any> {
        return cy.get('#deniedReasonInfo');
    }

    get requestedBy(): Cypress.Chainable<any> {
        return cy.get('#requestedByInfo');
    }
}

class DenyModal {

    get denyReason(): Cypress.Chainable<any> {
        return cy.get('#denyInput');
    }

    get denyButton(): Cypress.Chainable<any> {
        return cy.get('#denyButton');
    }
}

class MovieDetailsPage extends BasePage {

    get title(): Cypress.Chainable<any> {
        return cy.get('#mediaTitle');
    }

    get availableButton(): Cypress.Chainable<any> {
        return cy.get('#availableBtn');
    }

    get requestButton(): Cypress.Chainable<any> {
        return cy.get('#requestBtn');
    }

    get requestedButton(): Cypress.Chainable<any> {
        return cy.get('#requestedBtn');
    }

    get approveButton(): Cypress.Chainable<any> {
        return cy.get('#approveBtn');
    }

    get markAvailableButton(): Cypress.Chainable<any> {
        return cy.get('#markAvailableBtn');
    }

    get denyButton(): Cypress.Chainable<any> {
        return cy.get('#denyBtn');
    }

    get deniedButton(): Cypress.Chainable<any> {
        return cy.get('#deniedButton');
    }

    get reportIssueButton(): Cypress.Chainable<any> {
        return cy.get('#reportIssueBtn');
    }

    get viewCollectionButton(): Cypress.Chainable<any> {
        return cy.get('#viewCollectionBtn');
    }

    get viewOnPlexButton(): Cypress.Chainable<any> {
        return cy.get('#viewOnPlexButton');
    }

    get viewOnEmbyButton(): Cypress.Chainable<any> {
        return cy.get('#viewOnEmbyButton');
    }

    get viewOnJellyfinButton(): Cypress.Chainable<any> {
        return cy.get('#viewOnJellyfinButton');
    }

    denyModal = new DenyModal();
    informationPanel = new MovieInformationPanel();
    adminOptionsDialog = new AdminRequestDialog();

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/details/movie/` + id, options);
    }
}

export const movieDetailsPage = new MovieDetailsPage();
