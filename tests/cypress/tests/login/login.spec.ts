describe("Login Tests", () => {

  it("Landing Page is enabled, should redirect", () => {

    cy.landingSettings(true);
    cy.visit("");
    cy.location("pathname").should("eq", "/landingpage");
    cy.get("[data-cy=continue]").click();
    cy.location("pathname").should("contains", "/login");
  });

  it("Landing Page is disabled, should not redirect", () => {

    cy.landingSettings(false);
    cy.visit("");

    cy.location("pathname").should("eq", "/login");
  });

  it("Plex OAuth Enabled, should be button", () => {
    cy.landingSettings(false);
    cy.fixture('login/authenticationSettngs').then((settings)  => {
        settings.enableOAuth = true;
        cy.intercept('GET', '/Settings/Authentication', settings).as('authSettings')
      })

    cy.visit("");

    cy.get("[data-cy=oAuthPlexButton]").should('be.visible');
    cy.get("[data-cy=OmbiButton]").should('be.visible');
  });

  it("Plex OAuth Diabled, Should show local form", () => {
    cy.landingSettings(false);
    cy.fixture('login/authenticationSettngs').then((settings)  => {
        settings.enableOAuth = false;
        cy.intercept('GET', '/Settings/Authentication', settings).as('authSettings')
      })

    cy.visit("");

    cy.get("[data-cy=OmbiButton]").should('be.visible');

    cy.get('#username-field').should('be.visible');
    cy.get('#password-field').should('be.visible');
  });

  it('Invalid Password',() => {
    cy.landingSettings(false);
    cy.visit('/');
    cy.contains('Sign in');

    cy.get('#username-field').type('automation');
    cy.get('#password-field').type('incorrectpw');

    cy.get('[data-cy=OmbiButton]').click();
    cy.verifyNotification('Incorrect username');
  });

  it('Invalid Username',() => {
    cy.landingSettings(false);
    cy.visit('/');
    cy.contains('Sign in');

    cy.get('#username-field').type('bad username');
    cy.get('#password-field').type('incorrectpw');

    cy.get('[data-cy=OmbiButton]').click();
    cy.verifyNotification('Incorrect username');
  });

  it('Correct Login',() => {
    cy.landingSettings(false);
    cy.visit('/');
    cy.contains('Sign in');

    cy.get('#username-field').type(Cypress.env('username'));
    cy.get('#password-field').type(Cypress.env('password'));

    cy.get('[data-cy=OmbiButton]').click();
    cy.url().should('include', '/discover')
  });
});
