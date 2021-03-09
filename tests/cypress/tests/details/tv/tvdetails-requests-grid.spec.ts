describe("TV Requests Grid", function () {
  beforeEach(() => {
    cy.login();
  });

  it("Season not available", () => {
    cy.visit("/details/tv/121361");

    cy.getByData("classStatus1").should("not.have.class", "available");
    cy.getByData("classStatus1").should("not.have.class", "requested");
    cy.getByData("classStatus1").should("not.have.class", "approved");

    cy.getByData("masterCheckbox1").should("be.visible");
    cy.getByDataLike("episodeCheckbox1").each((element) => {
      expect(element.length).to.be.greaterThan(0);
    });
  });

  it("Season is requested", () => {
    cy.intercept("GET", "**/v2/search/Tv/121361", (req) => {
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

    cy.visit("/details/tv/121361");

    cy.wait("@detailsResponse");

    cy.getByData("classStatus1").should("not.have.class", "available");
    cy.getByData("classStatus1").should("have.class", "requested");
    cy.getByData("classStatus1").should("not.have.class", "approved");

    // checkboxes
    cy.getByData("masterCheckbox1").should("not.exist");
    cy.getByDataLike("episodeCheckbox1").should("not.exist");

    cy.getByDataLike("episodeStatus1").each((element) => {
      expect(element.hasClass("requested")).to.be.true;
      expect(element.text()).contain('Pending Approval');
    });
  });

  it("Season is approved", () => {
    cy.intercept("GET", "**/v2/search/Tv/121361", (req) => {
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

    cy.visit("/details/tv/121361");

    cy.wait("@detailsResponse");

    cy.getByData("classStatus1").should("not.have.class", "available");
    cy.getByData("classStatus1").should("not.have.class", "requested");
    cy.getByData("classStatus1").should("have.class", "approved");

    // checkboxes
    cy.getByData("masterCheckbox1").should("not.exist");
    cy.getByDataLike("episodeCheckbox1").should("not.exist");

    cy.getByDataLike("episodeStatus1").each((element) => {
      expect(element.hasClass("approved")).to.be.true;
      expect(element.text()).contain('Approved');
    });
  });

  it("Season is available", () => {
    cy.intercept("GET", "**/v2/search/Tv/121361", (req) => {
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

    cy.visit("/details/tv/121361");

    cy.wait("@detailsResponse");

    cy.getByData("classStatus1").should("have.class", "available");
    cy.getByData("classStatus1").should("not.have.class", "requested");
    cy.getByData("classStatus1").should("not.have.class", "approved");

    // checkboxes
    cy.getByData("masterCheckbox1").should("not.exist");
    cy.getByDataLike("episodeCheckbox1").should("not.exist");

    cy.getByDataLike("episodeStatus1").each((element) => {
      expect(element.hasClass("available")).to.be.true;
      expect(element.text()).contain('Available');
    });
  });

  it("Request no episodes", () => {
    cy.visit("/details/tv/121361");

    cy.get('#addFabBtn').click();
    cy.get('#requestSelected').click();

    cy.verifyNotification('You need to select some episodes!');
  });

  it("Request single episodes", () => {
    cy.visit("/details/tv/121361");
    const episodeCheckbox = 'episodeCheckbox11';

    cy.getByData(episodeCheckbox).click();
    cy.get('#addFabBtn').click();
    cy.get('#requestSelected').click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    cy.getByData('episodeStatus11')
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });


  it("Request First Season", () => {
    cy.visit("/details/tv/121361");

    cy.get('#addFabBtn').click();
    cy.get('#requestFirst').click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    cy.getByDataLike('episodeStatus1')
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });

  it("Request Latest Season", () => {
    cy.visit("/details/tv/121361");

    cy.get('#addFabBtn').click();
    cy.get('#requestLatest').click();

    cy.verifyNotification('Request for Game of Thrones has been added successfully');

    cy.getByData("classStatus8").click();
    cy.getByData("classStatus8").should("have.class", "requested");
    cy.getByDataLike('episodeStatus8')
      .should('contain.text', 'Pending Approval')
      .should('have.class', 'requested')
  });
});
