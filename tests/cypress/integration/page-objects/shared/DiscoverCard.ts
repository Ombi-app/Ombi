import { EpisodeRequestModal } from "./EpisodeRequestModal";

export enum DiscoverType {
  Upcoming,
  Trending,
  Popular,
  RecentlyRequested,
}

export class DiscoverCard {
    private id: string;
    private movie: boolean;
    private type: DiscoverType;

    episodeRequestModal = new EpisodeRequestModal();
    constructor(id: string, movie: boolean, type?: DiscoverType) {
      this.id = id;
      this.movie = movie;
      this.type = type;
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
      if (this.type) {
        return cy.get(`#requestButton${this.id}${this.movie ? '1' : '0'}${this.type}`);
      }

      return cy.get(`#requestButton${this.id}${this.movie ? '1' : '0'}`);
    }

    verifyTitle(expected: string): Cypress.Chainable<any> {
        return this.title.should('have.text',expected);
    }
}
