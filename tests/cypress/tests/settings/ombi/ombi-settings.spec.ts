import { generalSettingsPage as Page } from "@/integration/page-objects";

// The General ("Ombi") settings page is fully self-contained - it reads/writes
// only Ombi's own configuration with no external service - so it can be driven
// deterministically. Each assertion waits on an explicit signal (element render,
// intercepted response, or a retried value assertion) rather than a timeout.
describe("General Settings Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Renders the general settings form", () => {
    Page.visit();

    // The form only renders once the settings have loaded, so these checks
    // double as a deterministic "page ready" gate.
    Page.apiKey.should("be.visible");
    Page.baseUrl.should("be.visible");
    Page.submit.should("be.visible");
  });

  it("Refreshing the API key generates a new key", () => {
    Page.visit();

    Page.apiKey.should("be.visible").invoke("val").then((original) => {
      expect(String(original), "an API key should already be present").to.have.length.greaterThan(0);

      Page.refreshApiKey.click();

      // The field is patched with the freshly generated key; retry until it
      // differs from the original.
      Page.apiKey.invoke("val").should((updated) => {
        expect(String(updated)).to.have.length.greaterThan(0);
        expect(updated).to.not.equal(original);
      });
    });
  });

  it("Toggling a setting persists after saving", () => {
    cy.intercept("POST", "**/Settings/Ombi").as("saveSettings");

    Page.visit();
    Page.autoApproveToggle.should("be.visible");

    // Flip whatever the current state is, save, then re-load and confirm the new
    // state was persisted server-side. Reading the initial state keeps the test
    // idempotent across re-runs (it simply flips back and forth each time).
    Page.autoApproveToggle.then(($toggle) => {
      const wasChecked = $toggle.hasClass("mat-checked");

      Page.autoApproveToggle.click();
      Page.submit.click();

      cy.wait("@saveSettings").its("response.statusCode").should("eq", 200);
      cy.verifyNotification("Successfully saved Ombi settings");

      Page.visit();
      Page.autoApproveToggle.should(
        wasChecked ? "not.have.class" : "have.class",
        "mat-checked"
      );
    });
  });
});

export {};
