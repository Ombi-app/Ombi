import { After, Before, Given, When, Then } from "@badeball/cypress-cucumber-preprocessor";
import { loginPage as Page } from "@/integration/page-objects";

Given("I set the Landing Page to {string}", (bool: string) => {
  cy.landingSettings(bool === "true");
});

When("I visit Ombi", () => {
  Page.visit();
});

Then("I should be on the {string}", (string: string) => {
  cy.location("pathname").should("eq", `/${string}`);

});

Then("I click continue", () => {
  cy.get("[data-cy=continue]").click();
});
