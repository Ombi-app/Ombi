
export class AdminRequestDialog {

    isOpen(): Cypress.Chainable<any> { 
        return cy.waitUntil(x => {
            return this.title.should('exist');
        });
    }

    get title(): Cypress.Chainable<any> {
        return cy.get(`#advancedOptionsTitle`);
    }

    get requestOnBehalfUserInput(): Cypress.Chainable<any> {
        return cy.get(`#requestOnBehalfUserInput`);
    }

    get sonarrQualitySelect(): Cypress.Chainable<any> {
        return cy.get(`#sonarrQualitySelect`);
    }

    selectSonarrQuality(id: number): Cypress.Chainable<any> {
        return cy.get(`#sonarrQualitySelect${id}`);
    }

    get sonarrFolderSelect(): Cypress.Chainable<any> {
        return cy.get(`#sonarrFolderSelect`);
    }

    selectSonarrFolder(id: number): Cypress.Chainable<any> {
        return cy.get(`#sonarrFolderSelect${id}`);
    }

    get radarrQualitySelect(): Cypress.Chainable<any> {
        return cy.get(`#radarrQualitySelect`);
    }

    selectradarrQuality(id: number): Cypress.Chainable<any> {
        return cy.get(`#radarrQualitySelect${id}`);
    }

    get radarrFolderSelect(): Cypress.Chainable<any> {
        return cy.get(`#radarrFolderSelect`);
    }

    selectradarrFolder(id: number): Cypress.Chainable<any> {
        return cy.get(`#radarrFolderSelect${id}`);
    }


    get cancelButton(): Cypress.Chainable<any> {
        return cy.get(`#cancelButton`);
    }

    get requestButton(): Cypress.Chainable<any> {
        return cy.get(`#requestButton`);
    }
}
