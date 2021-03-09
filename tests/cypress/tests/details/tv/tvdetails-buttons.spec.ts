describe("TV Details Buttons", function () {
  beforeEach(() => {
    cy.login();
  });

  it("Fully Available Request", () => {

    cy.intercept("GET", "**/v2/search/Tv/121361", (req) => {
      req.reply((res) => {
        const body = res.body;
        body.fullyAvailable = true;
        body.partlyAvailable = false;
        res.send(body);
      });
    }).as("detailsResponse");

    cy.visit("/details/tv/121361");

    cy.wait('@detailsResponse');

    cy.get('#availableBtn').should('be.visible');
    cy.get('#requestBtn').should('not.exist');
    cy.get('#addFabBtn').should('not.exist');
  });

  it("Partially Available Request", () => {

    cy.intercept("GET", "**/v2/search/Tv/121361", (req) => {
      req.reply((res) => {
        const body = res.body;
        body.fullyAvailable = false;
        body.partlyAvailable = true;
        res.send(body);
      });
    }).as("detailsResponse");

    cy.visit("/details/tv/121361");

    cy.wait('@detailsResponse');

    cy.get('#availableBtn').should('not.exist');
    cy.get('#requestBtn').should('be.visible');
    cy.get('#addFabBtn').should('be.visible');
    cy.get('#partiallyAvailableBtn').should('be.visible');
  });

  it("Not Available Request", () => {

    cy.intercept("GET", "**/v2/search/Tv/121361", (req) => {
      req.reply((res) => {
        const body = res.body;
        body.fullyAvailable = false;
        body.partlyAvailable = false;
        res.send(body);
      });
    }).as("detailsResponse");

    cy.visit("/details/tv/121361");
    cy.wait('@detailsResponse');

    cy.get('#availableBtn').should('not.exist');
    cy.get('#requestBtn').should('be.visible');
    cy.get('#addFabBtn').should('be.visible');
    cy.get('#partiallyAvailableBtn').should('not.exist');
  });


  it("Issues Enabled", () => {
    cy.intercept("GET", "Settings/issuesenabled", 'true');

    cy.visit("/details/tv/121361");

    cy.get('#reportIssueBtn').should('be.visible');
  });

  it("Issues Disabled", () => {
    cy.intercept("GET", "Settings/issuesenabled", 'false');

    cy.visit("/details/tv/121361");

    cy.get('#reportIssueBtn').should('not.exist');
  });
});
