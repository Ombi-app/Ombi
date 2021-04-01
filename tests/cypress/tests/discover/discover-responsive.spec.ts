import { discoverPage as Page } from "@/integration/page-objects";
import { DiscoverType } from "@/integration/page-objects/shared/DiscoverCard";

const mobiles = ['samsung-s10', 'iphone-x', 'iphone-xr', 'iphone-8']

describe("Discover Responsive Tests", () => {
  beforeEach(() => {
    cy.login();
  });
  mobiles.forEach((size: any) => {
    // make assertions on the logo using
    // an array of different viewports
    it(`Should display card on ${size} screen`, () => {
      window.localStorage.setItem("DiscoverOptions2", "2");
      cy.intercept("GET", "**/search/Movie/Popular/**").as("moviePopular");
      cy.viewport(size);
      Page.visit();

      cy.wait("@moviePopular").then((intecept) => {
        const id = intecept.response.body[0].id;
        const card = Page.popularCarousel.getCard(id, true, DiscoverType.Popular);
        card.title.realHover();
        card.verifyTitle(intecept.response.body[0].title);
      })
    })
  })


});
