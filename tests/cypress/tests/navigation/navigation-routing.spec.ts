import { discoverPage as Page } from "@/integration/page-objects";

// navigation-bar.spec only asserts which nav items are *visible*. These assert
// the items actually *route* where they should (and that logout returns to the
// login page). All deterministic - pure client-side routing, no external data.
describe("Navigation Routing Tests", () => {
  beforeEach(() => {
    cy.login();
    Page.visit(); // lands on /discover
  });

  it("Requests nav item routes to the requests list", () => {
    Page.navbar.requests.click();
    cy.location("pathname").should("contain", "/requests-list");
  });

  it("User management nav item routes to user management", () => {
    Page.navbar.userManagement.click();
    cy.location("pathname").should("contain", "/usermanagement");
  });

  it("Settings nav item routes to settings", () => {
    Page.navbar.settings.click();
    cy.location("pathname").should("contain", "/Settings");
  });

  it("Discover nav item routes back to discover", () => {
    // Navigate away first so the discover click is a real transition.
    Page.navbar.requests.click();
    cy.location("pathname").should("contain", "/requests-list");

    Page.navbar.discover.click();
    cy.location("pathname").should("contain", "/discover");
  });

  it("Logging out returns to the login page", () => {
    Page.navbar.logout.click();
    cy.location("pathname").should("contain", "/login");
  });
});

export {};
