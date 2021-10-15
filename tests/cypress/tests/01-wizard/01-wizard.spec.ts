import { wizardPage as Page } from "@/integration/page-objects";

describe("Wizard Setup", () => {

  it("Wizard should be first page", () => {
    Page.visit();
    cy.location("pathname").should("contains", "/Wizard");
  });


  it.only("Finsh with no local user", () => {
    Page.visit();

    Page.welcomeTab.next.click();
    Page.mediaServerTab.next.click();
    Page.localUserTab.next.click();
    Page.ombiConfigTab.next.click();
    Page.finishButton.click();

    cy.verifyNotification("Username '' is invalid, can only contain letters or digits.")

    // Verify we end back up on the user page
    Page.matStepsHeader.then((items) => {

      const results = items.filter((index, html) => {
        var attributes = Cypress.$(html).attr('ng-reflect-index');
        return attributes === "2"; // 2nd index
      }).get()[0];

      console.log(results);

      var attr = Cypress.$(results).attr('ng-reflect-selected');
      assert.equal(attr, 'true');
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
