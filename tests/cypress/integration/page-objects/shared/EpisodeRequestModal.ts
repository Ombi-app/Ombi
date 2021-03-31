
export class EpisodeRequestModal {

    get allSeasonsButton(): Cypress.Chainable<any> {
        return cy.get(`#episodeModalAllSeasons`);
    }
    get firstSeasonButton(): Cypress.Chainable<any> {
        return cy.get(`#episodeModalFirstSeason`);
    }
    get latestSeasonButton(): Cypress.Chainable<any> {
        return cy.get(`#episodeModalLatestSeason`);
    }
}
