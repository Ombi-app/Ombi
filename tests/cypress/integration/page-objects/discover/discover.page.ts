import { BasePage } from "../base.page";

class DiscoverCard {
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
    return cy.get(`button > [data-test=requestButton${this.id}${this.movie ? '1' : '0'}]`);
  }

  verifyTitle(expected: string): Cypress.Chainable<any> {
      return this.title.should('have.text',expected);
  }
}

class CarouselComponent {
  private type: string;

  get combinedButton(): Cypress.Chainable<any> {
    return cy.get(`#${this.type}Combined-button`);
  }

  get movieButton(): Cypress.Chainable<any> {
    return cy.get(`#${this.type}Movie-button`);
  }

  get tvButton(): Cypress.Chainable<any> {
    return cy.get(`#${this.type}Tv-button`);
  }

  getCard(id: string, movie: boolean): DiscoverCard {
    return new DiscoverCard(id, movie);
  }

  constructor(id: string) {
    this.type = id;
  }
}

class DiscoverPage extends BasePage {
  popularCarousel = new CarouselComponent("popular");

  constructor() {
    super();
  }

  visit(options?: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow> {
    return cy.visit(`/discover`, options);
  }
}

export const discoverPage = new DiscoverPage();
