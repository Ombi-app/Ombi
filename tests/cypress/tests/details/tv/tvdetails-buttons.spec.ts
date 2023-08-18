import { tvDetailsPage as Page } from "@/integration/page-objects";

describe("TV Details Buttons", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Fully Available Request", () => {

    cy.intercept("GET", "**/v2/search/Tv/1399", (req) => {
      req.reply((res) => {
        const body = res.body;
        body.fullyAvailable = true;
        body.partlyAvailable = false;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit("1399");

    cy.wait('@detailsResponse');

    Page.availableButton.should('be.visible');
    Page.requestButton.should('not.exist');
    Page.requestFabButton.fab.should('not.exist');
  });

  it("Partially Available Request", () => {

    cy.intercept("GET", "**/v2/search/Tv/1399", (req) => {
      req.reply((res) => {
        const body = res.body;
        body.fullyAvailable = false;
        body.partlyAvailable = true;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit("1399");

    cy.wait('@detailsResponse');

    Page.availableButton.should('not.exist');
    Page.requestButton.should('be.visible');
    Page.requestFabButton.fab.should('be.visible');
    Page.partiallyAvailableButton.should('be.visible');
  });

  it("Not Available Request", () => {

    cy.intercept("GET", "**/v2/search/Tv/1399", (req) => {
      req.reply((res) => {
        const body = res.body;
        body.fullyAvailable = false;
        body.partlyAvailable = false;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit("1399");
    cy.wait('@detailsResponse');

    Page.availableButton.should('not.exist');
    Page.requestButton.should('be.visible');
    Page.requestFabButton.fab.should('be.visible');
    Page.partiallyAvailableButton.should('not.exist');
  });


  it.only("Issues Enabled", () => {
    cy.intercept("GET", "**/Settings/issuesenabled", 'true').as('issuesEnabled');

    cy.visit("/details/tv/1399");

    cy.wait('@issuesEnabled');
    Page.reportIssueButton(10000).should('be.visible');
  });

  it("Issues Disabled", () => {
    cy.intercept("GET", "**/Settings/issuesenabled", 'false');

    Page.visit("1399");

    Page.reportIssueButton(1000).should('not.exist');
  });
});
