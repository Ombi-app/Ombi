import { customizationSettingsPage as Page } from "@/integration/page-objects";

// Customization is one of the only fully self-contained settings pages: it has
// no external service dependency (no Plex/Sonarr/TMDb call, no Wiremock), so it
// can be exercised deterministically end-to-end against a real backend. Every
// assertion below waits on an explicit condition (element render, intercepted
// response, or a retried value assertion) rather than a fixed timeout.
describe("Customization Settings Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Renders the customization form", () => {
    Page.visit();

    // The form is only rendered once the settings have hydrated from the store,
    // so these visibility checks double as a deterministic "page loaded" gate.
    Page.applicationName.should("be.visible");
    Page.applicationUrl.should("be.visible");
    Page.submit.should("be.visible");
  });

  it("Saving a new application name persists it", () => {
    cy.intercept("POST", "**/Settings/customization").as("saveSettings");

    Page.visit();
    Page.applicationName.should("be.visible");

    cy.generateUniqueId().then((id) => {
      const name = `Ombi-${id}`;

      Page.applicationName.clear().type(name);
      Page.submit.click();

      // Assert the save round-tripped successfully before checking persistence.
      cy.wait("@saveSettings").its("response.statusCode").should("eq", 200);
      cy.verifyNotification("Successfully saved Ombi settings");

      // A full re-visit re-bootstraps the app and re-reads the value from the
      // server; the value assertion retries until the form has re-hydrated.
      Page.visit();
      Page.applicationName.should("have.value", name);
    });
  });
});

export {};
