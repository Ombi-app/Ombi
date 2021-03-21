import { searchBar as SearchBar } from './SearchBar';

class SearchFilter {

  constructor() { }

  get filterButton(): Cypress.Chainable<any> {
    return cy.get('#search-filter');
  }

  get moviesToggle(): Cypress.Chainable<any> {
    return cy.get('#filterMovies');
  }

  get tvToggle(): Cypress.Chainable<any> {
    return cy.get('#filterTv');
  }

  get musicToggle(): Cypress.Chainable<any> {
    return cy.get('#filterMusic');
  }

  applyFilter(tv: boolean, movies: boolean, music: boolean): void {
    window.localStorage.removeItem('searchFilter');
    window.localStorage.setItem('searchFilter', JSON.stringify({ movies: movies, music: music, tvShows: tv}));
  }
}

class NavBar {

  get profileImage(): Cypress.Chainable<any> {
    return cy.get('#profile-image');
  }

  get applicationName(): Cypress.Chainable<any> {
    return cy.get('#nav-applicationName');
  }

  get discover(): Cypress.Chainable<any> {
    return cy.get('#nav-discover');
  }

  get requests(): Cypress.Chainable<any> {
    return cy.get('#nav-requests');
  }

  get issues(): Cypress.Chainable<any> {
    return cy.get('#nav-issues');
  }

  get userManagement(): Cypress.Chainable<any> {
    return cy.get('#nav-userManagement');
  }

  get adminDonate(): Cypress.Chainable<any> {
    return cy.get('#nav-adminDonate');
  }

  get userDonate(): Cypress.Chainable<any> {
    return cy.get('#nav-userDonate');
  }

  get featureSuggestion(): Cypress.Chainable<any> {
    return cy.get('#nav-featureSuggestion');
  }

  get settings(): Cypress.Chainable<any> {
    return cy.get('#nav-settings');
  }

  get userPreferences(): Cypress.Chainable<any> {
    return cy.get('#profile-image');
  }

  get logout(): Cypress.Chainable<any> {
    return cy.get('#nav-logout');
  }


  constructor() { }

  searchBar = SearchBar;
  searchFilter = new SearchFilter();
}

export const navBar = new NavBar();