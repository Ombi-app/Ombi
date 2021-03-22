import { BasePage } from "../base.page";

class MediaBaseTab {
    get allRequestsButton(): Cypress.Chainable<any> {
        return cy.get('#filterAll');
    }

    get pendingRequestsButton(): Cypress.Chainable<any> {
        return cy.get('#filterPending');
    }

    get processingRequestsButton(): Cypress.Chainable<any> {
        return cy.get('#filterProcessing');
    }

    get availableRequestsButton(): Cypress.Chainable<any> {
        return cy.get('#filterAvailable');
    }

    get deniedRequestsButton(): Cypress.Chainable<any> {
        return cy.get('#filterDenied');
    }

    get requestsToDisplayDropdown(): Cypress.Chainable<any> {
        return cy.get('#requestsToDisplayDropdown');
    }

    getGridRow(requestId: number): GridRow {
        return new GridRow(requestId);
    }

}

class GridRow {
    requestId: number;
    get title(): Cypress.Chainable<any> {
        return cy.get(`#title${this.requestId}`);
    }

    get requestedBy(): Cypress.Chainable<any> {
        return cy.get(`#requestedBy${this.requestId}`);
    }

    get requestedDate(): Cypress.Chainable<any> {
        return cy.get(`#requestedDate${this.requestId}`);
    }

    get requestedStatus(): Cypress.Chainable<any> {
        return cy.get(`#requestedStatus${this.requestId}`);
    }

    get status(): Cypress.Chainable<any> {
        return cy.get(`#status${this.requestId}`);
    }

    get detailsButton(): Cypress.Chainable<any> {
        return cy.get(`#detailsButton${this.requestId}`);
    }

    get optionsButton(): Cypress.Chainable<any> {
        return cy.get(`#optionsButton${this.requestId}`);
    }

    get optionsDelete(): Cypress.Chainable<any> {
        return cy.get(`#requestDelete`);
    }

    get optionsApprove(): Cypress.Chainable<any> {
        return cy.get(`#requestApprove`);
    }

    get optionsChangeAvailability(): Cypress.Chainable<any> {
        return cy.get(`#requestChangeAvailability`);
    }

    constructor(requestId: number) {
        this.requestId = requestId;
    }
}

class MoviesTab extends MediaBaseTab {

    get adminMasterCheckbox(): Cypress.Chainable<any> {
        return cy.get('#adminMasterCheckbox');
    }

    get bulkFabButton(): Cypress.Chainable<any> {
        return cy.get('#bulkFab');
    }

    get deleteFabButton(): Cypress.Chainable<any> {
        return cy.get('#deleteFabButton');
    }

    get approveFabButton(): Cypress.Chainable<any> {
        return cy.get('#approveFabButton');
    }

    getRowCheckbox(rowId: number): Cypress.Chainable<any> {
        return cy.get('#adminMasterCheckbox' + rowId);
    }
}

class RequestsPage extends BasePage {

    get moviesTab(): Cypress.Chainable<any> {
        return cy.get('[role="tab"]').eq(0);
    }

    get tvTab(): Cypress.Chainable<any> {
        return cy.get('[role="tab"]').eq(1);
    }

    get musicTab(): Cypress.Chainable<any> {
        return cy.get('[role="tab"]').eq(2);
    }

    movies = new MoviesTab();
    tv = new MediaBaseTab();
    music = new MediaBaseTab();

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/requests-list`, options);
    }

}

export const requestPage = new RequestsPage();
