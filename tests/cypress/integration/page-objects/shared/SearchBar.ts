
class SearchBar {

  constructor() { }

    get searchInput(): Cypress.Chainable<any> {
        return cy.get(`#nav-search`);
    }

}

export const searchBar = new SearchBar();