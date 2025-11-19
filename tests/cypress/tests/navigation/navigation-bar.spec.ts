import { discoverPage as Page } from "@/integration/page-objects";

describe("Navigation Bar Tests", () => {
  it("Navigation Bar should show admin options when logged in as an admin", () => {
    cy.login();
    Page.visit();

    Page.navbar.adminDonate.should("be.visible");
    Page.navbar.settings.should("be.visible");
    Page.navbar.userManagement.should("be.visible");
    Page.navbar.requests.should("be.visible");
    Page.navbar.discover.should("be.visible");
    Page.navbar.userPreferences.should("be.visible");
    Page.navbar.username.contains("a");
    Page.navbar.logout.should("be.visible");
  });

  it("Navigation Bar should not show admin options when logged in as an non-admin", () => {
    cy.generateUniqueId().then((id) => {
      cy.login();
      const roles = [];
      roles.push({ value: "RequestMovie", enabled: true });
      cy.createUser(id, "a", roles).then(() => {
        cy.removeLogin();
        cy.loginWithCreds(id, "a");

        cy.intercept("GET", "api/v2/search/Movie/Popular/**").as("discoverLoad");
        Page.visit();

        cy.wait("@discoverLoad");

        Page.navbar.adminDonate.should("not.exist");
        Page.navbar.settings.should("not.exist");
        Page.navbar.userManagement.should("not.exist");
        Page.navbar.requests.should("be.visible");
        Page.navbar.discover.should("be.visible");
        Page.navbar.userPreferences.should("be.visible");
        Page.navbar.username.contains(id);
        Page.navbar.logout.should("be.visible");
      });
    });
  });
});

describe("Search Filter Badge Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Search filter badge should not be visible with default filters", () => {
    Page.navbar.searchFilter.applyFilter(true, true, false, false);
    Page.visit();

    Page.navbar.searchFilter.searchFilterBadge.should('not.exist');
  });

  it("Search filter badge should appear when a default-enabled filter is disabled", () => {
    Page.navbar.searchFilter.applyFilter(true, false, false, false);
    Page.visit();

    Page.navbar.searchFilter.searchFilterBadge.should('be.visible');
    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '1');
  });

  it("Search filter badge should appear when music is enabled", () => {
    Page.navbar.searchFilter.applyFilter(true, true, true, false);
    Page.visit();

    Page.navbar.searchFilter.searchFilterBadge.should('be.visible');
    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '3');
  });

  it("Search filter badge should appear when people is enabled", () => {
    Page.navbar.searchFilter.applyFilter(true, true, false, true);
    Page.visit();

    Page.navbar.searchFilter.searchFilterBadge.should('be.visible');
    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '3');
  });

  it("Search filter badge count should accurately reflect the number of active filters", () => {
    Page.navbar.searchFilter.applyFilter(false, true, false, false);
    Page.visit();

    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '1');

    Page.navbar.searchFilter.applyFilter(true, true, true, true);
    cy.reload();

    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '4');
  });

  it("Search filter badge count should update when toggling filters in the UI", () => {
    Page.navbar.searchFilter.applyFilter(true, false, false, false);
    Page.visit();

    Page.navbar.searchFilter.searchFilterBadge.should('be.visible');
    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '1');

    Page.navbar.searchFilter.filterButton.click();
    Page.navbar.searchFilter.tvToggle.click();

    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '0');

    Page.navbar.searchFilter.moviesToggle.click();

    Page.navbar.searchFilter.searchFilterBadgeCount.should('eq', '1');
  });
});
