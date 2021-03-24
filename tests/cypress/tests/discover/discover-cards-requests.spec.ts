import { discoverPage as Page } from "@/integration/page-objects";

describe("Discover Cards Requests Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Not requested movie allows us to request", () => {
    window.localStorage.setItem("DiscoverOptions2", "2");
    cy.intercept("GET", "**/search/Movie/Popular/**", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[0];
        movie.available = false;
        movie.approved = false;
        movie.requested = false;

        body[0] = movie;
        res.send(body);
      });
    }).as("cardsResponse");

    Page.visit();

    cy.wait("@cardsResponse").then((res) => {
      const body = JSON.parse(res.response.body);
      var expectedId = body[0].id;
      var title = body[0].title;

      const card = Page.popularCarousel.getCard(expectedId, true);
      card.verifyTitle(title);
      card.requestButton.should("exist");
      // Not visible until hover
      card.requestButton.should("not.be.visible");
      cy.wait(500)
      card.topLevelCard.realHover();

      card.requestButton.should("be.visible");
      card.requestButton.click();

      cy.verifyNotification("has been successfully added!");

      card.requestButton.should("not.be.visible");
      card.availabilityText.should('have.text','Pending');
      card.statusClass.should('have.class','requested');
    });
  });

  it("Available movie does not allow us to request", () => {
    window.localStorage.setItem("DiscoverOptions2", "2");
    cy.intercept("GET", "**/search/Movie/Popular/**", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[1];
        movie.available = true;
        movie.approved = false;
        movie.requested = false;

        body[1] = movie;
        res.send(body);
      });
    }).as("cardsResponse");

    Page.visit();

    cy.wait("@cardsResponse").then((res) => {
      const body = JSON.parse(res.response.body);
      var expectedId = body[1].id;
      var title = body[1].title;

      const card = Page.popularCarousel.getCard(expectedId, true);
      card.verifyTitle(title);
      card.topLevelCard.realHover();

      card.requestButton.should("not.exist");
      card.availabilityText.should('have.text','Available');
      card.statusClass.should('have.class','available');
    });
  });

  it("Requested movie does not allow us to request", () => {
    window.localStorage.setItem("DiscoverOptions2", "2");
    cy.intercept("GET", "**/search/Movie/Popular/**", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[1];
        movie.available = false;
        movie.approved = false;
        movie.requested = true;

        body[1] = movie;
        res.send(body);
      });
    }).as("cardsResponse");

    Page.visit();

    cy.wait("@cardsResponse").then((res) => {
      const body = JSON.parse(res.response.body);
      var expectedId = body[1].id;
      var title = body[1].title;

      const card = Page.popularCarousel.getCard(expectedId, true);
      card.verifyTitle(title);

      card.topLevelCard.realHover();

      card.requestButton.should("not.exist");
      card.availabilityText.should('have.text','Pending');
      card.statusClass.should('have.class','requested');
    });
  });

  it("Approved movie does not allow us to request", () => {
    window.localStorage.setItem("DiscoverOptions2", "2");
    cy.intercept("GET", "**/search/Movie/Popular/**", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[1];
        movie.available = false;
        movie.approved = true;
        movie.requested = true;

        body[1] = movie;
        res.send(body);
      });
    }).as("cardsResponse");

    Page.visit();

    cy.wait("@cardsResponse").then((res) => {
      const body = JSON.parse(res.response.body);
      var expectedId = body[1].id;
      var title = body[1].title;

      const card = Page.popularCarousel.getCard(expectedId, true);
      card.verifyTitle(title);
      card.topLevelCard.realHover();

      card.requestButton.should("not.exist");
      card.availabilityText.should('have.text','Approved');
      card.statusClass.should('have.class','approved');
    });
  });

  it("Available TV does not allow us to request", () => {
    cy.intercept("GET", "**/search/Tv/popular/**", (req) => {
      req.reply((res) => {
        const body = res.body;
        const tv = body[1];
        tv.fullyAvailable = true;

        body[1] = tv;
        res.send(body);
      });
    }).as("cardsResponse");
    window.localStorage.setItem("DiscoverOptions2", "3");

    Page.visit();

    cy.wait("@cardsResponse").then((res) => {
      const body = JSON.parse(res.response.body);
      var expectedId = body[1].id;
      var title = body[1].title;

      const card = Page.popularCarousel.getCard(expectedId, true);
      card.verifyTitle(title);
      card.topLevelCard.realHover();

      card.requestButton.should("not.exist");
      card.availabilityText.should('have.text','Available');
      card.statusClass.should('have.class','available');
    });
  });

  it("Not available TV does not allow us to request", () => {
    cy.intercept("GET", "**/search/Tv/popular/**", (req) => {
      req.reply((res) => {
        const body = res.body;
        const tv = body[3];
        tv.fullyAvailable = false;

        body[3] = tv;
        res.send(body);
      });
    }).as("cardsResponse");
    window.localStorage.setItem("DiscoverOptions2", "3");

    Page.visit();

    cy.wait("@cardsResponse").then((res) => {
      const body = JSON.parse(res.response.body);
      var expectedId = body[3].id;
      var title = body[3].title;

      const card = Page.popularCarousel.getCard(expectedId, false);
      card.verifyTitle(title);
      card.topLevelCard.realHover();

      card.requestButton.should("be.visible");
      card.requestButton.click();
      const modal = card.episodeRequestModal;

      modal.latestSeasonButton.click();
      cy.verifyNotification("has been added successfully")
    });
  });
});
