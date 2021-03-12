import { tvDetailsPage as Page } from "@/integration/page-objects";

describe("TV Requests Grid", function () {
  beforeEach(() => {
    cy.login();
  });

  it("Season not available", () => {
    Page.visit('1399');

    Page.requestPanel.seasonTab(1)
      .should("not.have.class", "available")
      .should("not.have.class", "requested")
      .should("not.have.class", "approved");

    Page.requestPanel.getSeasonMasterCheckbox(1).should("be.visible");
    Page.requestPanel.getEpisodeCheckbox(1).each((element) => {
      expect(element.length).to.be.greaterThan(0);
    });
  });

  it("Season is requested", () => {
    cy.intercept("GET", "**/v2/search/Tv/1399", (req) => {
      req.reply((res) => {
        const body = res.body;
        const requests = body.seasonRequests[0].episodes;
        requests.forEach((req) => {
          req.requested = true;
          req.approved = false;
          req.requestStatus = "Common.PendingApproval";
        });
        body.seasonRequests[0].episodes = requests;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit('1399');

    cy.wait("@detailsResponse");

    Page.requestPanel.seasonTab(1)
      .should("not.have.class", "available")
      .should("have.class", "requested")
      .should("not.have.class", "approved");

    // checkboxes
    Page.requestPanel.getSeasonMasterCheckbox(1).should("not.exist");
    Page.requestPanel.getEpisodeCheckbox(1).should("not.exist");

    Page.requestPanel.getEpisodeStatus(1).each((element) => {
      expect(element.hasClass("requested")).to.be.true;
      expect(element.text()).contain('Pending Approval');
    });
  });

  it("Season is approved", () => {
    cy.intercept("GET", "**/v2/search/Tv/1399", (req) => {
      req.reply((res) => {
        const body = res.body;
        const requests = body.seasonRequests[0].episodes;
        requests.forEach((req) => {
          req.approved = true;
          req.requested = true;
          req.requestStatus = "Common.Approved";
        });
        body.seasonRequests[0].episodes = requests;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit('1399');

    cy.wait("@detailsResponse");

    Page.requestPanel.seasonTab(1)
      .should("not.have.class", "available")
      .should("not.have.class", "requested")
      .should("have.class", "approved");

    // checkboxes
    Page.requestPanel.getSeasonMasterCheckbox(1).should("not.exist");
    Page.requestPanel.getEpisodeCheckbox(1).should("not.exist");

    Page.requestPanel.getEpisodeStatus(1).each((element) => {
      expect(element.hasClass("approved")).to.be.true;
      expect(element.text()).contain('Approved');
    });
  });

  it("Season is available", () => {
    cy.intercept("GET", "**/v2/search/Tv/1399", (req) => {
      req.reply((res) => {
        const body = res.body;
        const requests = body.seasonRequests[0].episodes;
        requests.forEach((req) => {
          req.available = true;
          req.requestStatus = "Common.Available";
        });
        body.seasonRequests[0].episodes = requests;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit('1399');

    cy.wait("@detailsResponse");

    Page.requestPanel.seasonTab(1)
      .should("have.class", "available")
      .should("not.have.class", "requested")
      .should("not.have.class", "approved");

    // checkboxes
    Page.requestPanel.getSeasonMasterCheckbox(1).should("not.exist");
    Page.requestPanel.getEpisodeCheckbox(1).should("not.exist");

    Page.requestPanel.getEpisodeStatus(1).each((element) => {
      expect(element.hasClass("available")).to.be.true;
      expect(element.text()).contain('Available');
    });
  });

  it("Request no episodes", () => {
    Page.visit('1399');

    Page.requestFabButton.fab.click();
    Page.requestFabButton.requestSelected.click();

    cy.verifyNotification('You need to select some episodes!');
  });

  it("Request single episodes", () => {
    Page.visit('1399');

    Page.requestPanel.seasonTab(2).click();
    Page.requestPanel.getEpisodeSeasonCheckbox(2,1).click();
    Page.requestFabButton.fab.click();
    Page.requestFabButton.requestSelected.click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    Page.requestPanel.getEpisodeStatus(2,1)
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });


  it("Request First Season", () => {
    Page.visit('1399');

    Page.requestFabButton.fab.click();
    Page.requestFabButton.requestFirst.click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    Page.requestPanel.getEpisodeStatus(1)
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });

  it("Request Latest Season", () => {
    Page.visit('1399');

    Page.requestFabButton.fab.click();
    Page.requestFabButton.requestLatest.click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    Page.requestPanel.seasonTab(8)
    .click()
    .should("have.class", "requested");

    Page.requestPanel.getEpisodeStatus(8)
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });
});
