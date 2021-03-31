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

        cy.intercept("GET", "search/Movie/Popular").as("discoverLoad");
        Page.visit();

        cy.wait("@discoverLoad");

        Page.navbar.adminDonate.should("not.exist");
        Page.navbar.settings.should("not.exist");
        Page.navbar.userManagement.should("not.exist");
        Page.navbar.requests.should("be.visible");
        Page.navbar.discover.should("be.visible");
        Page.navbar.userPreferences.should("be.visible");
        Page.navbar.logout.should("be.visible");
      });
    });
  });
});
