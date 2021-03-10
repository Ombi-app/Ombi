import { BasePage } from "../base.page";

class CarouselComponent {
    private id: string;


    get combinedButton(): Cypress.Chainable<any> {
        return cy.get(`#${this.id}Combined-button`);
    }

    get movieButton(): Cypress.Chainable<any> {
        return cy.get(`#${this.id}Movie-button`);
    }

    get tvButton(): Cypress.Chainable<any> {
        return cy.get(`#${this.id}Tv-button`);
    }

    constructor(id: string) {
        this.id = id;
    }
}

class DiscoverPage extends BasePage {

    popularCarousel = new CarouselComponent('popular');

    constructor() {
        super();
    }

    visit(options?: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow> {
        return cy.visit(`/discover`, options);
    }
}

export const discoverPage = new DiscoverPage();
