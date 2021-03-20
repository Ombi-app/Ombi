
export class DiscoverCard {
    private id: string;
    private movie: boolean;
    constructor(id: string, movie: boolean) {
      this.id = id;
      this.movie = movie;
    }

    get topLevelCard(): Cypress.Chainable<any> {
        return cy.get(`#result${this.id}`);
    }

    get requestType(): Cypress.Chainable<any> {
      return cy.get(`#type${this.id}`);
    }

    get statusClass(): Cypress.Chainable<any> {
      return cy.get(`#status${this.id}`);
    }

    get availabilityText(): Cypress.Chainable<any> {
      return cy.get(`#availabilityStatus${this.id}`);
    }

    get title(): Cypress.Chainable<any> {
      return cy.get(`#title${this.id}`);
    }

    get overview(): Cypress.Chainable<any> {
      return cy.get(`#overview${this.id}`);
    }

    get requestButton(): Cypress.Chainable<any> {
      return cy.get(`#requestButton${this.id}${this.movie ? '1' : '0'}`);
    }

    verifyTitle(expected: string): Cypress.Chainable<any> {
        return this.title.should('have.text',expected);
    }
}
