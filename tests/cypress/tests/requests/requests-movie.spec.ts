import {
  requestPage as Page,
  movieDetailsPage as MoviePage,
} from "@/integration/page-objects";

// The requests list previously only had TV coverage (navigation + delete). These
// mirror that for the movies tab. Movie requests made by an admin are persisted
// for real; the global before hook (cy.clearAllRequests) guarantees the grid
// starts empty so the requested movie is the only - and therefore visible - row.
describe("Movie Requests Tests", () => {
  const dbID = 315635; // Spider-Man: Homecoming

  beforeEach(() => {
    cy.login();
  });

  it("Clicking Details on a movie request, takes us to the correct detail page", () => {
    cy.requestMovie(dbID);

    Page.visit();
    Page.moviesTab.click();

    cy.waitUntil(() => {
      const row = Page.movies.getGridRow(dbID);
      return row.detailsButton.should("be.visible");
    });
    const row = Page.movies.getGridRow(dbID);
    row.detailsButton.click();

    cy.location("pathname").should("contains", "/details/movie/" + dbID);
    MoviePage.title.contains("Spider-Man");
  });

  it("Deleting a movie request, removes it from the grid", () => {
    cy.intercept("DELETE", "**/Request/movie/*").as("deleteRequest");
    cy.requestMovie(dbID);

    Page.visit();
    Page.moviesTab.click();

    const row = Page.movies.getGridRow(dbID);
    cy.waitUntil(() => row.optionsButton.should("be.visible"));
    row.optionsButton.click();
    row.optionsDelete.click();

    cy.wait("@deleteRequest").its("response.statusCode").should("eq", 200);
    row.title.should("not.exist");
  });
});

export {};
