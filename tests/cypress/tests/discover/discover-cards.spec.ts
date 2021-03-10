import { discoverPage as Page } from '@/integration/page-objects';

describe("Discover Cards Tests", () => {
  beforeEach(() => {
    cy.login();
  });

  it("Popular combined should load movies and TV", () => {

    cy.intercept("GET","**/search/Movie/Popular/**").as('moviePopular');
    cy.intercept("GET","**/search/Tv/popular/**").as('tvPopular');
    Page.visit();


    cy.wait('@moviePopular');
    cy.wait('@tvPopular');

  });

  it("Popular Movie should load movies", () => {

    cy.intercept("GET","**/search/Movie/Popular/**").as('moviePopular');
    Page.visit();
    Page.popularCarousel.movieButton.click();

    cy.wait('@moviePopular');

  });

  it.only("Popular TV should load TV", () => {

    cy.intercept("GET","**/search/Tv/popular/**").as('tvPopular');
    Page.visit();
    Page.popularCarousel.tvButton.click();

    cy.wait('@tvPopular');
  });

  it("Popular Moives selected when set in localstorage", () => {

    window.localStorage.setItem("DiscoverOptions2","2");
    cy.intercept("GET","**/search/Movie/Popular/**").as('moviePopular');
    Page.visit();
    Page.popularCarousel.movieButton.parent().should('have.class','button-active');

    cy.wait('@moviePopular');

  });

  it("Popular Tv selected when set in localstorage", () => {

    window.localStorage.setItem("DiscoverOptions2","3");
    cy.intercept("GET","**/search/Tv/popular/**").as('tvPopular');
    Page.visit();
    Page.popularCarousel.tvButton.parent().should('have.class','button-active');

    cy.wait('@tvPopular');

  });

  it("Popular Combined selected when set in localstorage", () => {

    window.localStorage.setItem("DiscoverOptions2","1");
    cy.intercept("GET","**/search/Movie/Popular/**").as('moviePopular');
    cy.intercept("GET","**/search/Tv/popular/**").as('tvPopular');
    Page.visit();
    Page.popularCarousel.combinedButton.parent().should('have.class','button-active');

    cy.wait('@moviePopular');
    cy.wait('@tvPopular');

  });

  });
