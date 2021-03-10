import { loginPage as Page } from "@/integration/page-objects";

describe("Login Tests", () => {
  it("Landing Page is enabled, should redirect", () => {
    cy.landingSettings(true);
    Page.visit();
    cy.location("pathname").should("eq", "/landingpage");
    cy.get("[data-cy=continue]").click();
    cy.location("pathname").should("contains", "/login");
  });

  it("Landing Page is disabled, should not redirect", () => {
    cy.landingSettings(false);
    Page.visit();

    cy.location("pathname").should("eq", "/login");
  });

  it("Plex OAuth Enabled, should be button", () => {
    cy.landingSettings(false);
    cy.fixture("login/authenticationSettngs").then((settings) => {
      settings.enableOAuth = true;
      cy.intercept("GET", "/Settings/Authentication", settings).as(
        "authSettings"
      );
    });

    Page.visit();

    Page.plexSignInButton.should("be.visible");
    Page.ombiSignInButton.should("be.visible");
  });

  it("Plex OAuth Diabled, Should show local form", () => {
    cy.landingSettings(false);
    cy.fixture("login/authenticationSettngs").then((settings) => {
      settings.enableOAuth = false;
      cy.intercept("GET", "/Settings/Authentication", settings).as(
        "authSettings"
      );
    });

    Page.visit();

    Page.ombiSignInButton.should("be.visible");

    Page.username.should("be.visible");
    Page.password.should("be.visible");
  });

  it("Invalid Password", () => {
    cy.landingSettings(false);

    Page.visit();
    cy.contains("Sign in");

    Page.username.type("automation");
    Page.password.type("incorrectpw");

    Page.ombiSignInButton.click();
    cy.verifyNotification("Incorrect username");
  });

  it("Invalid Username", () => {
    cy.landingSettings(false);

    Page.visit();
    cy.contains("Sign in");

    Page.username.type("bad username");
    Page.password.type("incorrectpw");

    Page.ombiSignInButton.click();
    cy.verifyNotification("Incorrect username");
  });

  it("Correct Login", () => {
    cy.landingSettings(false);
    Page.visit();
    cy.contains("Sign in");

    Page.username.type(Cypress.env("username"));
    Page.password.type(Cypress.env("password"));

    Page.ombiSignInButton.click();
    cy.url().should("include", "/discover");
  });
});
