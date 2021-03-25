import { discoverPage as Page } from "@/integration/page-objects";
import { DiscoverType } from "@/integration/page-objects/shared/DiscoverCard";

describe("Discover Cards Requests Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Not requested movie allows admin to request", () => {
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

      const card = Page.popularCarousel.getCard(expectedId, true, DiscoverType.Popular);
      card.verifyTitle(title);
      card.requestButton.should("exist");
      // Not visible until hover
      card.requestButton.should("not.be.visible");
      cy.wait(500);
      card.topLevelCard.realHover();

      card.requestButton.should("be.visible");
      card.requestButton.click();

      Page.adminOptionsDialog.isOpen();
      Page.adminOptionsDialog.requestButton.click();

      cy.verifyNotification("has been successfully added!");

      card.requestButton.should("not.exist");
      card.availabilityText.should("have.text", "Pending");
      card.statusClass.should("have.class", "requested");
    });
  });

  it.only("Not requested movie allows non-admin to request", () => {
    cy.generateUniqueId().then((id) => {
      cy.login();
      const roles = [];
      roles.push({ value: "RequestMovie", enabled: true });
      cy.createUser(id, "a", roles).then(() => {
        cy.removeLogin();
        cy.loginWithCreds(id, "a");

        window.localStorage.setItem("DiscoverOptions2", "2");
        cy.intercept("GET", "**/search/Movie/Popular/**", (req) => {
          req.reply((res) => {
            const body = res.body;
            const movie = body[6];
            movie.available = false;
            movie.approved = false;
            movie.requested = false;

            body[6] = movie;
            res.send(body);
          });
        }).as("cardsResponse");

        Page.visit();

        cy.wait("@cardsResponse").then((res) => {
          const body = JSON.parse(res.response.body);
          var expectedId = body[6].id;
          var title = body[6].title;

          const card = Page.popularCarousel.getCard(expectedId, true, DiscoverType.Popular);
          card.verifyTitle(title);
          card.requestButton.should("exist");
          // Not visible until hover
          card.requestButton.should("not.be.visible");
          cy.wait(500);
          card.topLevelCard.realHover();

          card.requestButton.should("be.visible");
          card.requestButton.click();

          cy.verifyNotification("has been successfully added!");

          card.requestButton.should("not.exist");
          card.availabilityText.should("have.text", "Pending");
          card.statusClass.should("have.class", "requested");
        });
      });
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

      const card = Page.popularCarousel.getCard(expectedId, true, DiscoverType.Popular);
      card.verifyTitle(title);
      card.topLevelCard.realHover();

      card.requestButton.should("not.exist");
      card.availabilityText.should("have.text", "Available");
      card.statusClass.should("have.class", "available");
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

      const card = Page.popularCarousel.getCard(expectedId, true, DiscoverType.Popular);
      card.title.realHover();

      card.verifyTitle(title);
      card.requestButton.should("not.exist");
      card.availabilityText.should("have.text", "Pending");
      card.statusClass.should("have.class", "requested");
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

      const card = Page.popularCarousel.getCard(expectedId, true, DiscoverType.Popular);
      card.title.realHover();

      card.verifyTitle(title);
      card.requestButton.should("not.exist");
      card.availabilityText.should("have.text", "Approved");
      card.statusClass.should("have.class", "approved");
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

      const card = Page.popularCarousel.getCard(expectedId, true, DiscoverType.Popular);
      card.title.realHover();

      card.verifyTitle(title);
      card.requestButton.should("not.exist");
      card.availabilityText.should("have.text", "Available");
      card.statusClass.should("have.class", "available");
    });
  });

  it("Not available TV allow admin to request", () => {
    cy.intercept("GET", "**/search/Tv/popular/**", (req) => {
      req.reply((res) => {
        const body = res.body;
        const tv = body[3];
        tv.fullyAvailable = false;

        body[3] = tv;
        res.send(body);
      });
    }).as("cardsResponse");
    cy.intercept("GET", "**/search/Tv/**").as("otherResponses");
    window.localStorage.setItem("DiscoverOptions2", "3");

    Page.visit();

    cy.wait("@otherResponses");
    cy.wait("@cardsResponse").then((res) => {
      const body = JSON.parse(res.response.body);
      var expectedId = body[3].id;
      var title = body[3].title;

      const card = Page.popularCarousel.getCard(expectedId, false, DiscoverType.Popular);
      card.title.realHover();

      card.verifyTitle(title);
      card.requestButton.should("be.visible");
      card.requestButton.click();
      const modal = card.episodeRequestModal;

      modal.latestSeasonButton.click();

      Page.adminOptionsDialog.isOpen();
      Page.adminOptionsDialog.requestButton.click();

      cy.verifyNotification("has been added successfully");
    });
  });

  it("Not available TV allow non-admin to request", () => {
    cy.generateUniqueId().then((id) => {
      cy.login();
      const roles = [];
      roles.push({ value: "RequestTv", enabled: true });
      cy.createUser(id, "a", roles).then(() => {
        cy.removeLogin();
        cy.loginWithCreds(id, "a");

        cy.intercept("GET", "**/search/Tv/popular/**", (req) => {
          req.reply((res) => {
            const body = res.body;
            const tv = body[5];
            tv.fullyAvailable = false;

            body[5] = tv;
            res.send(body);
          });
        }).as("cardsResponse");
        cy.intercept("GET", "**/search/Tv/**").as("otherResponses");
        window.localStorage.setItem("DiscoverOptions2", "3");

        Page.visit();

        cy.wait("@otherResponses");
        cy.wait("@cardsResponse").then((res) => {
          const body = JSON.parse(res.response.body);
          var expectedId = body[5].id;
          var title = body[5].title;

          const card = Page.popularCarousel.getCard(expectedId, false, DiscoverType.Popular);
          card.title.realHover();

          card.verifyTitle(title);
          card.requestButton.should("be.visible");
          card.requestButton.click();
          const modal = card.episodeRequestModal;

          modal.latestSeasonButton.click();

          cy.verifyNotification("has been added successfully");
        });
      });
    });
  });
});
