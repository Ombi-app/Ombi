export abstract class BasePage {
    abstract visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    abstract visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
}
