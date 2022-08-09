import { userPreferencesPage as Page } from "@/integration/page-objects";

describe("User Preferences Profile Tests", () => {
  beforeEach(() => {
    cy.login();
  });

const langs = [
  { code: 'fr', discover: 'Découvrir'},
  { code: 'de', discover: 'Entdecken'},
  { code: 'en', discover: 'Discover'},
];

langs.forEach((l) => {
  it.only(`Change language to ${l.code}, UI should update`, () => {
    cy.intercept('POST','**/language').as('langSave');
    Page.visit();

    Page.profile.languageSelectBox.click();
    Page.profile.languageSelectBoxOption(l.code).click();

    Page.navbar.discover.contains(l.discover);

    cy.wait('@langSave').then((intercept) => {
      expect(intercept.request.body.lang).equal(l.code);
    })
  });
})

const streamingCountries = [
  'GB',
  'US',
  'FR',
  'HU'
];

streamingCountries.forEach((country) => {
    // derive test name from data
    it(`Change streaming to ${country} UI should update`, () => {
      cy.intercept('GET','streamingcountry').as('countryApi');
      cy.intercept('POST','streamingcountry').as('countryApiSave');
      Page.visit();
      cy.wait('@countryApi');

      Page.profile.streamingSelectBox.click();
      Page.profile.streamingSelectBoxOption(country).click();

      cy.wait('@countryApiSave').then((intercept) => {
        expect(intercept.request.body.code).equal(country);
      })
    })
  })


});
