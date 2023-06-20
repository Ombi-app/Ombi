
import { discoverPage as Page } from "@/integration/page-objects";

describe("Discover Recently Requested Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Requested Movie Is Displayed", () => {

    cy.requestMovie(315635);
    cy.intercept("GET", "**/v2/Requests/recentlyRequested").as("response");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("315635");
      card.verifyTitle("Spider-Man: Homecoming");
      card.status.should('contain.text', 'Approved'); 
    });
  });

  it("Requested Movie Is Pending Approval", () => {

    cy.requestMovie(626735);

    cy.intercept("GET", "**/v2/Requests/recentlyRequested", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[0];
        movie.available = false;
        movie.approved = false;

        body[0] = movie;
        res.send(body);
      });
    }).as("response");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("626735");
      card.verifyTitle("Dog");
      card.status.should('contain.text', 'Pending');
      card.approveButton.should('be.visible');
    });
  });

  it("Requested Movie Is Available", () => {

    cy.requestMovie(675353);

    cy.intercept("GET", "**/v2/Requests/recentlyRequested", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[0];
        movie.available = true;

        body[0] = movie;
        res.send(body);
      });
    }).as("response");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("675353");
      card.verifyTitle("Sonic the Hedgehog 2");
      card.status.should('contain.text', 'Available'); // Because admin auto request
      card.approveButton.should('not.exist');
    });
  });

  it("Requested TV Is Available", () => {

    cy.requestAllTv(135647);

    cy.intercept("GET", "**/v2/Requests/recentlyRequested", (req) => {
      req.reply((res) => {
        const body = res.body;
        const tv = body[0];
        tv.available = true;

        body[0] = tv;
        res.send(body);
      });
    }).as("response");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("135647");
      card.verifyTitle("2 Good 2 Be True");
      card.status.should('contain.text', 'Available');
      card.approveButton.should('not.exist');
    });
  });

  it("Requested TV Is Partially Available", () => {

    cy.requestAllTv(158415);

    cy.intercept("GET", "**/v2/Requests/recentlyRequested", (req) => {
      req.reply((res) => {
        const body = res.body;
        const tv = body[0];
        tv.tvPartiallyAvailable = true;

        body[0] = tv;
        res.send(body);
      });
    }).as("response");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("158415");
      card.verifyTitle("Pantanal");
      card.status.should('contain.text', 'Partially Available');
      card.approveButton.should('not.exist');
    });
  });

  it("Requested TV Is Pending", () => {
    cy.requestAllTv(60574);

    cy.intercept("GET", "**/v2/Requests/recentlyRequested", (req) => {
      req.reply((res) => {
        const body = res.body;
        const tv = body[0];
        tv.approved = false;

        body[0] = tv;
        res.send(body);
      });
    }).as("response");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("60574");
      card.verifyTitle("Peaky Blinders");
      card.status.should('contain.text', 'Pending');
      card.approveButton.should('be.visible');
    });
  });

  it("Requested TV Is Displayed", () => {

    cy.requestAllTv(66732);
    cy.intercept("GET", "**/v2/Requests/recentlyRequested").as("response");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("66732");
      card.verifyTitle("Stranger Things");
      card.status.should('contain.text', 'Approved'); // Because admin auto request
    });
  });

  it("Approve Requested Movie", () => {

    cy.requestMovie(55341);

    cy.intercept("GET", "**/v2/Requests/recentlyRequested", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[0];
        movie.available = false;
        movie.approved = false;

        body[0] = movie;
        res.send(body);
      });
    }).as("response");

    cy.intercept("POST", "**/v1/Request/Movie/Approve").as("approveCall");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("55341");
      card.approveButton.should('be.visible');
      card.approveButton.click();

      cy.wait("@approveCall").then((_) => {
        card.status.should('contain.text', 'Approved');
      });

    });
  });

  it("Approve Requested Tv Show", () => {

    cy.requestAllTv(71712);

    cy.intercept("GET", "**/v2/Requests/recentlyRequested", (req) => {
      req.reply((res) => {
        const body = res.body;
        const movie = body[0];
        movie.available = false;
        movie.approved = false;

        body[0] = movie;
        res.send(body);
      });
    }).as("response");

    cy.intercept("POST", "**/v1/Request/tv/approve").as("approveCall");

    Page.visit();

    cy.wait("@response").then((_) => {

      const card = Page.recentlyRequested.getRequest("71712");
      card.approveButton.should('be.visible');
      card.approveButton.click();

      cy.wait("@approveCall").then((_) => {
        card.status.should('contain.text', 'Approved');
      });

    });
  });

});
