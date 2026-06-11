import { featuresSettingsPage as Page } from "@/integration/page-objects";

// The Features settings page is a list of self-contained toggles (no external
// service) that persist immediately via an enable/disable call. We drive the
// "PlayedSync" feature because it is side-effect-free in a test environment.
describe("Features Settings Tests", () => {
  const feature = "PlayedSync";

  beforeEach(() => {
    cy.login();
  });

  it("Renders the feature toggles", () => {
    Page.visit();
    Page.featureToggle(feature).should("be.visible");
  });

  it("Toggling a feature persists after reload", () => {
    cy.intercept("POST", "**/Features/enable").as("enableFeature");
    cy.intercept("POST", "**/Features/disable").as("disableFeature");

    Page.visit();
    Page.featureToggle(feature).should("be.visible");

    // Flip the current state and wait on the matching persistence call, then
    // re-load and confirm the new state stuck. Reading the initial state keeps
    // the test idempotent across re-runs.
    Page.featureToggle(feature).then(($toggle) => {
      const wasEnabled = $toggle.hasClass("mat-checked");

      Page.featureToggle(feature).click();
      cy.wait(wasEnabled ? "@disableFeature" : "@enableFeature")
        .its("response.statusCode")
        .should("eq", 200);

      Page.visit();
      Page.featureToggle(feature).should(
        wasEnabled ? "not.have.class" : "have.class",
        "mat-checked"
      );
    });
  });
});

export {};
