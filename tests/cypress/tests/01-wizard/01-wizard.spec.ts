import { wizardPage as Page } from "@/integration/page-objects";

describe("Wizard Setup", () => {

  it("Wizard should be first page", () => {
    Page.visit();
    cy.location("pathname").should("contains", "/Wizard");
  });


  it("Finsh with no local user", () => {
    Page.visit();

    Page.welcomeTab.next.click();
    Page.mediaServerTab.next.click();
    Page.localUserTab.next.click();
    Page.ombiConfigTab.next.click();
    Page.finishButton.click();

    cy.verifyNotification("Username '' is invalid, can only contain letters or digits.")

    // Verify we end back up on the user page
    Page.matStepsHeader.then((items) => {

      cy.get('#cdk-step-label-0-2').should('have.attr', 'aria-selected', 'true');

    });

  });

  it("Compete Wizard", () => {
    Page.visit();

    Page.welcomeTab.next.click();
    Page.mediaServerTab.next.click();

    Page.localUserTab.username.type(Cypress.env("username"));
    Page.localUserTab.password.type(Cypress.env("password"));

    Page.localUserTab.next.click();
    Page.ombiConfigTab.next.click();

    Page.finishButton.click();

    cy.location("pathname").should("contains", "/login");
  });
});
