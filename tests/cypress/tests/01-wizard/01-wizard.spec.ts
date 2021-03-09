describe("Wizard Setup", () => {

  it("Wizard should be first page", () => {
    cy.visit("");
    cy.location("pathname").should("contains", "/Wizard");
  });


  it("Finsh with no local user", () => {
    cy.visit("");
    cy.get("[data-test=nextWelcome]").click();
    cy.get("[data-test=nextMediaServer]").click();
    cy.get("[data-test=nextLocalUser]").click();
    cy.get("[data-test=nextOmbiConfig]").click();
    cy.get("#finishWizard").click();

    cy.verifyNotification("Username '' is invalid, can only contain letters or digits.")

    // Verify we end back up on the user page
    cy.get('mat-step-header').then((items) => {

      const results = items.filter((index, html) => {
        var attributes = Cypress.$(html).attr('ng-reflect-index');
        return attributes === "2"; // 2nd index
      }).get()[0];

      console.log(results);

      var attr = Cypress.$(results).attr('ng-reflect-selected');
      assert.equal(attr, 'true');
    });

  });

  it.only("Compete Wizard", () => {
    cy.visit("");
    cy.get("[data-test=nextWelcome]").click();
    cy.get("[data-test=nextMediaServer]").click();

    cy.get('#adminUsername').type(Cypress.env("username"));
    cy.get('#adminPassword').type(Cypress.env("password"));

    cy.get("[data-test=nextLocalUser]").click();
    cy.get("[data-test=nextOmbiConfig]").click();
    cy.get("#finishWizard").click();

    cy.location("pathname").should("contains", "/login");
  });
});
