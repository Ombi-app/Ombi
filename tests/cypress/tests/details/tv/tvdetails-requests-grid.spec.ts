import { tvDetailsPage as Page } from "@/integration/page-objects";

describe("TV Requests Grid", function () {
  beforeEach(() => {
    cy.login();
  });

  it("Season not available", () => {
    Page.visit('1399');

    Page.requestPanel.seasonChip(1)
      .should("not.have.class", "available")
      .should("not.have.class", "partial")
      .should("not.have.class", "denied");

    Page.requestPanel.getSelectAllToggle(1).should("be.visible");
    Page.requestPanel.getEpisodeCheckbox(1).each((element) => {
      expect(element.length).to.be.greaterThan(0);
    });
  });

  it("Season is requested", () => {
    cy.intercept("GET", "**/v2/search/Tv/1399", (req) => {
      req.reply((res) => {
        const body = res.body;
        const requests = body.seasonRequests[0].episodes;
        requests.forEach((episode: any) => {
          episode.requested = true;
          episode.approved = false;
          episode.requestStatus = "Common.PendingApproval";
        });
        body.seasonRequests[0].episodes = requests;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit('1399');

    cy.wait("@detailsResponse");

    Page.requestPanel.seasonChip(1)
      .should("have.class", "partial");

    // checkboxes should not exist for requested episodes
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
        requests.forEach((episode: any) => {
          episode.approved = true;
          episode.requested = true;
          episode.requestStatus = "Common.Approved";
        });
        body.seasonRequests[0].episodes = requests;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit('1399');

    cy.wait("@detailsResponse");

    Page.requestPanel.seasonChip(1)
      .should("have.class", "partial");

    // checkboxes should not exist for approved episodes
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
        requests.forEach((episode: any) => {
          episode.available = true;
          episode.requestStatus = "Common.Available";
        });
        body.seasonRequests[0].episodes = requests;
        res.send(body);
      });
    }).as("detailsResponse");

    Page.visit('1399');

    cy.wait("@detailsResponse");

    Page.requestPanel.seasonChip(1)
      .should("have.class", "available");

    // checkboxes should not exist for available episodes
    Page.requestPanel.getEpisodeCheckbox(1).should("not.exist");

    Page.requestPanel.getEpisodeStatus(1).each((element) => {
      expect(element.hasClass("available")).to.be.true;
      expect(element.text()).contain('Available');
    });
  });

  it("Request no episodes", () => {
    Page.visit('1399');

    // The request selected button should not be visible when no episodes are selected
    Page.requestButtons.requestSelected.should('not.exist');
  });

  it("Request single episodes", () => {
    Page.visit('1399');

    Page.requestPanel.seasonChip(2).click();
    Page.requestPanel.getEpisodeSeasonCheckbox(2,1).click();
    Page.requestButtons.requestSelected.click();

    Page.adminOptionsDialog.isOpen();
    Page.adminOptionsDialog.requestButton.click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    Page.requestPanel.getEpisodeStatus(2,1)
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')

    Page.requestPanel.getEpisodeStatus(2).each((element) => {
      if (element.attr('data-test') !== 'episodeStatus21') {
        expect(element.hasClass("requested")).to.be.false;
      }
    });
  });


  it("Request First Season", () => {
    Page.visit('1399');

    Page.requestButtons.requestFirst.click();

    Page.adminOptionsDialog.isOpen();
    Page.adminOptionsDialog.requestButton.click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    Page.requestPanel.getEpisodeStatus(1)
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });

  it("Request Latest Season", () => {
    Page.visit('1399');

    Page.requestButtons.requestLatest.click();

    Page.adminOptionsDialog.isOpen();
    Page.adminOptionsDialog.requestButton.click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    Page.requestPanel.seasonChip(8)
    .click()
    .should("have.class", "partial");

    Page.requestPanel.getEpisodeStatus(8)
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });
});

export {};
