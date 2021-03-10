export abstract class BasePage {
    abstract visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
}
