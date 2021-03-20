import { BasePage } from "../base.page";
import { DiscoverCard } from "../shared/DiscoverCard";

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

  visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
  visit(): Cypress.Chainable<Cypress.AUTWindow>;
  visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
  visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
  visit(id?: any, options?: any) {
    return cy.visit(`/discover`, options);
  }
}

export const discoverPage = new DiscoverPage();
