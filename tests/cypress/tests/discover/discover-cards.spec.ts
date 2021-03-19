import { discoverPage as Page } from "@/integration/page-objects";

describe("Discover Cards Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Popular combined should load movies and TV", () => {
    cy.intercept("GET", "**/search/Movie/Popular/**").as("moviePopular");
    cy.intercept("GET", "**/search/Tv/popular/**").as("tvPopular");
    Page.visit();

    cy.wait("@moviePopular");
    cy.wait("@tvPopular");
  });

  it("Popular Movie should load movies", () => {
    cy.intercept("GET", "**/search/Movie/Popular/**").as("moviePopular");
    Page.visit();
    Page.popularCarousel.movieButton.click();

    cy.wait("@moviePopular");
  });

  it("Popular TV should load TV", () => {
    cy.intercept("GET", "**/search/Tv/popular/**").as("tvPopular");
    Page.visit();
    Page.popularCarousel.tvButton.click();

    cy.wait("@tvPopular");
  });

  it("Popular Moives selected when set in localstorage", () => {
    window.localStorage.setItem("DiscoverOptions2", "2");
    cy.intercept("GET", "**/search/Movie/Popular/**").as("moviePopular");
    Page.visit();
    Page.popularCarousel.movieButton
      .parent()
      .should("have.class", "button-active");

    cy.wait("@moviePopular");
  });

  it("Popular Tv selected when set in localstorage", () => {
    window.localStorage.setItem("DiscoverOptions2", "3");
    cy.intercept("GET", "**/search/Tv/popular/**").as("tvPopular");
    Page.visit();
    Page.popularCarousel.tvButton
      .parent()
      .should("have.class", "button-active");

    cy.wait("@tvPopular");
  });

  it("Popular Combined selected when set in localstorage", () => {
    window.localStorage.setItem("DiscoverOptions2", "1");
    cy.intercept("GET", "**/search/Movie/Popular/**").as("moviePopular");
    cy.intercept("GET", "**/search/Tv/popular/**").as("tvPopular");
    Page.visit();
    Page.popularCarousel.combinedButton
      .parent()
      .should("have.class", "button-active");

    cy.wait("@moviePopular");
    cy.wait("@tvPopular");
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
  
        card.requestButton.should("not.be.visible");
        card.availabilityText.should('have.text','Approved');
        card.statusClass.should('have.class','approved');
      });
    });
  });
});
