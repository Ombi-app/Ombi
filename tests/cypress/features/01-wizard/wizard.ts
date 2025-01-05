import { Given, When, Then } from "@badeball/cypress-cucumber-preprocessor";
import { wizardPage as Page } from "@/integration/page-objects";

Given("I set the Landing Page to {string}", (bool) => {
  cy.landingSettings(bool);
});

When("I visit Ombi", () => {
  Page.visit();
});

When("I click through all of the pages", () => {
  Page.welcomeTab.next.click();
  Page.databaseTab.next.click();
  Page.mediaServerTab.next.click();
  Page.localUserTab.next.click();
  Page.ombiConfigTab.next.click();
 });

 When("I finish the Wizard", () => {
  Page.finishButton.click();
 });

 When("I click through to the user page", () => {
    Page.welcomeTab.next.click();
    Page.databaseTab.next.click();
    Page.mediaServerTab.next.click();
 });

 When("I enter a username", () => {
  Page.localUserTab.username.type(Cypress.env("username"));
 });

 When("I enter a password", () => {
  Page.localUserTab.password.type(Cypress.env("password"));
 });

 When("I go to the finished tab", () => {
  Page.localUserTab.next.click();
  Page.ombiConfigTab.next.click();
 });

Then("I should be on the {string}", (string) => {
  cy.location("pathname").should("eq", `/${string}`);
});

Then("I should get a notification {string}", (string) => {
  cy.verifyNotification(string);
});

Then("I should be on the User tab", () => {
  Page.matStepsHeader.then((_) => {
    cy.get('#cdk-step-label-0-3').should('have.attr', 'aria-selected', 'true');
  });
});