import { searchPage as Page } from "@/integration/page-objects";

describe("Search Tests", () => {
  beforeEach(() => {
    cy.login();
    cy.intercept("POST", "v2/search/multi/").as("searchResponse");
  });

  it("Single result when TV Search Only", () => {
    Page.navbar.searchFilter.applyFilter(true, false, false);
    Page.visit("Dexters Laboratory");

    cy.wait('@searchResponse');
    const card = Page.getCard('4229', false);

    card.topLevelCard.realHover();
    card.title.should('have.text', "Dexter's Laboratory");
    card.overview.contains('Dexter');
    card.requestType.contains('TV Show');
    card.requestButton.should('exist');
  });

  it("No results when bad TV search", () => {
    Page.navbar.searchFilter.applyFilter(true, false, false);
    Page.visit("Game Of Thrones a aba aba aba aba");

    cy.wait('@searchResponse');

    Page.noSearchResultMessage.should('exist');
  });

  it("No results when bad Movie search", () => {
    Page.navbar.searchFilter.applyFilter(false, true, false);
    Page.visit("Game Of Thrones a aba aba aba aba");

    cy.wait('@searchResponse');

    Page.noSearchResultMessage.should('exist');
  });

  it("No results when bad Music search", () => {
    Page.navbar.searchFilter.applyFilter(false, false, true);
    Page.visit("Game Of Thrones a aba aba aba aba");

    cy.wait('@searchResponse');

    Page.noSearchResultMessage.should('exist');
  });

  it("No results when bad combined search", () => {
    Page.navbar.searchFilter.applyFilter(true, true, true);
    Page.visit("Game Of Thrones a aba aba aba aba");

    cy.wait('@searchResponse');

    Page.noSearchResultMessage.should('exist');
  });

  it("No results when bad tv + movie search", () => {
    Page.navbar.searchFilter.applyFilter(true, true, false);
    Page.visit("Game Of Thrones a aba aba aba aba");

    cy.wait('@searchResponse');

    Page.noSearchResultMessage.should('exist');
  });

  it("Single result when Movie Search Only", () => {
    Page.navbar.searchFilter.applyFilter(false, true, false);
    Page.visit("half blood prince");

    cy.wait('@searchResponse');
    const card = Page.getCard('767', true);

    card.topLevelCard.realHover();
    card.title.should('have.text', 'Harry Potter and the Half-Blood Prince (2009)');
    card.overview.contains('Hogwarts');
    card.requestType.contains('Movie');
    card.requestButton.should('exist');
  });

  it("No TV results, enabling movies filter we get results", () => {
    Page.navbar.searchFilter.applyFilter(true, false, false);
    Page.visit("half blood prince");

    cy.wait('@searchResponse');
    Page.noSearchResultMessage.should('exist');

    Page.navbar.searchFilter.filterButton.click();
    Page.navbar.searchFilter.moviesToggle.click();

    cy.wait('@searchResponse');
    const card = Page.getCard('767', true);

    card.topLevelCard.realHover();
    card.title.should('have.text', 'Harry Potter and the Half-Blood Prince (2009)');
    card.overview.contains('Hogwarts');
    card.requestType.contains('Movie');
    card.requestButton.should('exist');
  });

  it("No Movie results, enabling Tv filter we get results", () => {
    Page.navbar.searchFilter.applyFilter(false, true, false);
    Page.visit("It's always sunny in Philadelphia");

    cy.wait('@searchResponse');
    Page.noSearchResultMessage.should('exist');

    Page.navbar.searchFilter.filterButton.click();
    Page.navbar.searchFilter.tvToggle.click();

    cy.wait('@searchResponse');
    const card = Page.getCard('2710', false);

    card.topLevelCard.realHover();
    card.title.should('have.text', "It's Always Sunny in Philadelphia");
    card.overview.contains('Irish pub');
    card.requestType.contains('TV Show');
    card.requestButton.should('exist');
  });

  it("Multiple Movie Matches", () => {
    Page.navbar.searchFilter.applyFilter(false, true, false);
    Page.visit("007");

    cy.wait('@searchResponse');

    Page.searchResultsContainer.invoke('attr', 'search-count').then((x: string) => {
      expect(+x).to.be.greaterThan(0);
    });
  });

  it("Multiple Tv Matches", () => {
    Page.navbar.searchFilter.applyFilter(true, false, false);
    Page.visit("net");

    cy.wait('@searchResponse');

    Page.searchResultsContainer.invoke('attr', 'search-count').then((x: string) => {
      expect(+x).to.be.greaterThan(0);
    });
  });

  it("Multiple combined Matches", () => {
    Page.navbar.searchFilter.applyFilter(true, true, false);
    Page.visit("net");

    cy.wait('@searchResponse');

    Page.searchResultsContainer.invoke('attr', 'search-count').then((x: string) => {
      expect(+x).to.be.greaterThan(0);
    });
  });

  it("Searching via the search bar", () => {
    Page.navbar.searchFilter.applyFilter(true, true, false);
    Page.visit(" ");

    cy.wait('@searchResponse');

    Page.navbar.searchBar.searchInput.type('007');

    Page.searchResultsContainer.invoke('attr', 'search-count').then((x: string) => {
      expect(+x).to.be.greaterThan(0);
    });
  });
});
