import { BasePage } from "../base.page";
import { AdminRequestDialog } from "../shared/AdminRequestDialog";
import { DiscoverCard, DiscoverType } from "../shared/DiscoverCard";

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

  getCard(id: string, movie: boolean, type?: DiscoverType): DiscoverCard {
    return new DiscoverCard(id, movie, type);
  }

  constructor(id: string) {
    this.type = id;
  }
}

class RecentlyRequestedComponent {
  getRequest(id: string): DetailedCard {
    return new DetailedCard(id);
  }
}

class DetailedCard {
  private id: string;

  // Root card element. The action buttons (approve/deny) are only revealed when
  // the card is hovered or focused, so tests must interact with this first via
  // reveal() before asserting on / clicking those buttons.
  get container(): Cypress.Chainable<any> {
    return cy.get(`#detailed-${this.id}`);
  }

  get title(): Cypress.Chainable<any> {
    return cy.get(`#detailed-request-title-${this.id}`);
  }

  get status(): Cypress.Chainable<any> {
    return cy.get(`#detailed-request-status-${this.id}`);
  }

  get approveButton(): Cypress.Chainable<any> {
    return cy.get(`#detailed-request-approve-${this.id}`);
  }

  // Reveal the hover-only action buttons. Focusing the card triggers the same
  // `:focus-within` rule the design uses for hover, which is far more
  // deterministic in a headless run than synthesising a real mouse hover.
  reveal(): Cypress.Chainable<any> {
    return this.container.focus();
  }

  verifyTitle(expected: string): Cypress.Chainable<any> {
    return this.title.should('have.text',expected);
  }

  constructor(id: string) {
    this.id = id;
  }
}

class DiscoverPage extends BasePage {
  popularCarousel = new CarouselComponent("popular");
  recentlyRequested = new RecentlyRequestedComponent();
  adminOptionsDialog = new AdminRequestDialog();

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
