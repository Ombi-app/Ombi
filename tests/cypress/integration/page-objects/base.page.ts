
import { navBar as NavBar } from './shared/NavBar';
export abstract class BasePage {
    abstract visit(options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;
    abstract visit(): Cypress.Chainable<Cypress.AUTWindow>;
    abstract visit(id: string): Cypress.Chainable<Cypress.AUTWindow>;
    abstract visit(id: string, options: Cypress.VisitOptions): Cypress.Chainable<Cypress.AUTWindow>;

    navbar = NavBar;
}
