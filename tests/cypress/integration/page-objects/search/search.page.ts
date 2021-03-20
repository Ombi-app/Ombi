import { BasePage } from "../base.page";
import { DiscoverCard } from "../shared/DiscoverCard";

class SearchPage extends BasePage {

    get noSearchResultMessage(): Cypress.Chainable<any> {
        return cy.get('#noSearchResult');
    }

    get searchResultsContainer(): Cypress.Chainable<any> {
        return cy.get('#searchResults');
    }

    get searchResultCount(): Cypress.Chainable<any> {
        return cy.get('[search-count*=]');
    }

    getCard(id: string, movie: boolean): DiscoverCard {
        return new DiscoverCard(id, movie);
      }

    constructor() {
        super();
    }

    visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    visit(id?: any, options?: any) {
        return cy.visit(`/discover/` + encodeURI(id), options);
    }
}

export const searchPage = new SearchPage();
