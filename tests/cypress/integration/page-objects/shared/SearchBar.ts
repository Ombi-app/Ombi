
class SearchBar {

  constructor() { }

    get searchButton(): Cypress.Chainable<any> {
        return cy.get(`#nav-search-btn`);
    }

    get searchInput(): Cypress.Chainable<any> {
        return cy.get(`#nav-search`);
    }

}

export const searchBar = new SearchBar();
